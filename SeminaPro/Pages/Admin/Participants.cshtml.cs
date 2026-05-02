using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SeminaPro.Data;
using SeminaPro.Models;

namespace SeminaPro.Pages.Admin
{
    public class ParticipantsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ParticipantsModel> _logger;

        public List<Participant>? Participants { get; set; }
        public List<Specialite>? Specialites { get; set; }

        [BindProperty]
        public Participant Participant { get; set; } = new();

        public ParticipantsModel(ApplicationDbContext context, ILogger<ParticipantsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult OnGet(int? id)
        {
            CheckAdmin();
            LoadSpecialites();

            if (id.HasValue)
            {
                Participant = _context.Participants
                    .Include(p => p.Specialite)
                    .FirstOrDefault(p => p.Id == id.Value);
                if (Participant == null)
                    return NotFound();
            }

            LoadParticipants();
            return Page();
        }

        public IActionResult OnPostSave()
        {
            CheckAdmin();
            LoadSpecialites();

            if (!ModelState.IsValid)
            {
                LoadParticipants();
                return Page();
            }

            try
            {
                if (Participant.Id == 0)
                {
                    _context.Participants.Add(Participant);
                    _logger.LogInformation($"Participant {Participant.Nom} {Participant.Prenom} créé");
                }
                else
                {
                    _context.Participants.Update(Participant);
                    _logger.LogInformation($"Participant {Participant.Nom} {Participant.Prenom} modifié");
                }

                _context.SaveChanges();
                TempData["Message"] = Participant.Id == 0 ? "Participant créé avec succès" : "Participant modifié avec succès";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du participant");
                TempData["Error"] = "Erreur lors de la sauvegarde";
                LoadParticipants();
                return Page();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            CheckAdmin();

            try
            {
                var participant = _context.Participants.FirstOrDefault(p => p.Id == id);
                if (participant == null)
                    return NotFound();

                // Supprimer les inscriptions associées
                var inscriptions = _context.Inscriptions.Where(i => i.ParticipantId == id).ToList();
                _context.Inscriptions.RemoveRange(inscriptions);

                _context.Participants.Remove(participant);
                _context.SaveChanges();

                TempData["Message"] = "Participant supprimé avec succès";
                _logger.LogInformation($"Participant {participant.Nom} {participant.Prenom} supprimé");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du participant");
                TempData["Error"] = "Erreur lors de la suppression";
            }

            return RedirectToPage();
        }

        private void LoadParticipants()
        {
            try
            {
                Participants = _context.Participants
                    .Include(p => p.Specialite)
                    .Include(p => p.Inscriptions)
                    .OrderBy(p => p.Nom)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des participants");
                Participants = new List<Participant>();
            }
        }

        private void LoadSpecialites()
        {
            try
            {
                Specialites = _context.Specialites.OrderBy(s => s.Libelle).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement des spécialités");
                Specialites = new List<Specialite>();
            }
        }

        private void CheckAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                throw new UnauthorizedAccessException("Accès réservé aux administrateurs");
            }
        }
    }
}
