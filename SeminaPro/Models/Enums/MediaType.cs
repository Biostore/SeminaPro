namespace SeminaPro.Models.Enums
{
    /// <summary>
    /// Énumération des types de fichiers médias
    /// </summary>
    public enum MediaType
    {
        /// <summary>Image de profil utilisateur</summary>
        ProfileImage = 0,

        /// <summary>Image du séminaire</summary>
        SeminaireImage = 1,

        /// <summary>Document de séminaire (PDF, Word, etc.)</summary>
        SeminaireDocument = 2,

        /// <summary>Certificat d'attendance</summary>
        Certificate = 3,

        /// <summary>Image de spécialité (logo/icône)</summary>
        SpecialityImage = 4,

        /// <summary>Autre document</summary>
        Other = 5
    }
}
