using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutOfSchool.Services.Models;

    public class AreaAdmin : InstitutionAdminBase, IKeyedEntity<(string, long)>
    {
        [Required(ErrorMessage = "CATOTTGId is required")]
        public long CATOTTGId { get; set; }

        public virtual CATOTTG CATOTTG { get; set; }

        [NotMapped]
        public (string, long) Id
        {
            get => (UserId, CATOTTGId);
            set
            {
                UserId = value.Item1;
                CATOTTGId = value.Item2;
            }
        }
    }
