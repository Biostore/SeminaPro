namespace SeminaPro.Models.Enums
{
    /// <summary>
    /// Énumération des types d'utilisateurs du système
    /// </summary>
    public enum UserType
    {
        /// <summary>Administrateur - Accès complet au système</summary>
        Admin = 0,

        /// <summary>Organisateur - Crée et gère les séminaires</summary>
        Organisateur = 1,

        /// <summary>Participant - S'inscrit aux séminaires</summary>
        Participant = 2,

        /// <summary>Modérateur - Modère les contenus et utilisateurs</summary>
        Moderateur = 3,

        /// <summary>Invité - Accès en lecture seule</summary>
        Invite = 4
    }
}
