using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace SeminaPro.Pages
{
    public class ContactModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Le prénom est requis")]
        public string? FirstName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Le nom est requis")]
        public string? LastName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "L'email n'est pas valide")]
        public string? Email { get; set; }

        [BindProperty]
        [Phone(ErrorMessage = "Le numéro de téléphone n'est pas valide")]
        public string? Phone { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Le sujet est requis")]
        public string? Subject { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Le message est requis")]
        [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères")]
        public string? ContactMessage { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vous devez accepter les conditions")]
        public bool Terms { get; set; }

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Veuillez remplir tous les champs correctement.";
                return Page();
            }

            try
            {
                // Here you would typically save the contact form to database or send an email
                // For now, we'll just show a success message

                // Example: You could send an email here
                // await SendContactEmailAsync(FirstName, LastName, Email, Phone, Subject, ContactMessage);

                Message = $"Merci {FirstName} ! Votre message a été envoyé avec succès. Nous vous répondrons bientôt à l'adresse {Email}.";

                // Clear the form
                FirstName = string.Empty;
                LastName = string.Empty;
                Email = string.Empty;
                Phone = string.Empty;
                Subject = string.Empty;
                ContactMessage = string.Empty;
                Terms = false;

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur s'est produite lors de l'envoi de votre message. Veuillez réessayer.";
                // Log the error here
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Page();
            }
        }

        // Optional: Method to send email
        // private async Task SendContactEmailAsync(string firstName, string lastName, string email, string phone, string subject, string message)
        // {
        //     // Implement email sending logic here
        // }
    }
}
