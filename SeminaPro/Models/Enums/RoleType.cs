namespace SeminaPro.Models.Enums
{
    /// <summary>
    /// Énumération des rôles du système RBAC (Role-Based Access Control)
    /// </summary>
    public enum RoleType
    {
        /// <summary>Admin - Gestion complète du système</summary>
        Admin = 0,

        /// <summary>Organisateur - Création et édition de séminaires</summary>
        Organisateur = 1,

        /// <summary>Participant - Inscription à séminaires</summary>
        Participant = 2,

        /// <summary>Modérateur - Modération des contenus</summary>
        Moderateur = 3,

        /// <summary>Invité - Lecture seule</summary>
        Invite = 4
    }
}
