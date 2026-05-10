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

        public List<Participant> Participants { get; set; } = new();

        public List<Specialite> Specialites { get; set; } = new();

        [BindProperty]
        public Participant Participant { get; set; } = new();

        public ParticipantsModel(
            ApplicationDbContext context,
            ILogger<ParticipantsModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================================
        // GET
        // =========================================

        public IActionResult OnGet(int? id)
        {
            try
            {
                CheckAdmin();

                LoadParticipants();
                LoadSpecialites();

                // MODE MODIFICATION
                if (id.HasValue)
                {
                    var participant = _context.Participants
                        .FirstOrDefault(p => p.Id == id.Value);

                    if (participant != null)
                    {
                        Participant = participant;
                    }
                }

                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GET Participants");
                TempData["Error"] = ex.Message;

                LoadParticipants();
                LoadSpecialites();

                return Page();
            }
        }

        // =========================================
        // SAVE
        // =========================================

        public IActionResult OnPostSave()
        {
            try
            {
                CheckAdmin();

                LoadParticipants();
                LoadSpecialites();

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Veuillez remplir tous les champs";

                    return Page();
                }

                // AJOUT
                if (Participant.Id == 0)
                {
                    _context.Participants.Add(Participant);

                    TempData["Message"] =
                        "Participant ajouté avec succès";
                }

                // MODIFICATION
                else
                {
                    var participantDb = _context.Participants
                        .FirstOrDefault(p => p.Id == Participant.Id);

                    if (participantDb == null)
                    {
                        TempData["Error"] =
                            "Participant introuvable";

                        return Page();
                    }

                    participantDb.Nom =
                        Participant.Nom;

                    participantDb.Prenom =
                        Participant.Prenom;

                    participantDb.Email =
                        Participant.Email;

                    participantDb.NumeroTelephone =
                        Participant.NumeroTelephone;

                    participantDb.SpecialiteId =
                        Participant.SpecialiteId;

                    TempData["Message"] =
                        "Participant modifié avec succès";
                }

                // IMPORTANT
                _context.SaveChanges();

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur lors de la sauvegarde");

                TempData["Error"] =
                    ex.InnerException?.Message ?? ex.Message;

                LoadParticipants();
                LoadSpecialites();

                return Page();
            }
        }

        // =========================================
        // DELETE
        // =========================================

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                CheckAdmin();

                var participant = _context.Participants
                    .FirstOrDefault(p => p.Id == id);

                if (participant == null)
                {
                    TempData["Error"] =
                        "Participant introuvable";

                    return RedirectToPage();
                }

                // SUPPRIMER INSCRIPTIONS
                var inscriptions = _context.Inscriptions
                    .Where(i => i.ParticipantId == id)
                    .ToList();

                _context.Inscriptions
                    .RemoveRange(inscriptions);

                // SUPPRIMER PARTICIPANT
                _context.Participants
                    .Remove(participant);

                _context.SaveChanges();

                TempData["Message"] =
                    "Participant supprimé avec succès";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Erreur suppression participant");

                TempData["Error"] =
                    ex.InnerException?.Message ?? ex.Message;
            }

            return RedirectToPage();
        }

        // =========================================
        // LOAD PARTICIPANTS
        // =========================================

        private void LoadParticipants()
        {
            Participants = _context.Participants
                .Include(p => p.Specialite)
                .Include(p => p.Inscriptions)
                .OrderByDescending(p => p.Id)
                .ToList();
        }

        // =========================================
        // LOAD SPECIALITES
        // =========================================

        private void LoadSpecialites()
        {
            Specialites = _context.Specialites
                .OrderBy(s => s.Libelle)
                .ToList();
        }

        // =========================================
        // CHECK ADMIN
        // =========================================

        private void CheckAdmin()
        {
            var userRole =
                HttpContext.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}