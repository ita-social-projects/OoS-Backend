using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class BirthCertificate
    {
        [Key]
        [ForeignKey("Child")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets series of birth certificate.
        /// </summary>
        public string SvidSer { get; set; }

        /// <summary>
        /// Gets or sets number of birth certificate.
        /// </summary>
        public string SvidNum { get; set; }

        public string SvidNumMD5 { get; set; }

        /// <summary>
        /// Gets or sets authority that issued the birth certificate.
        /// </summary>
        public string SvidWho { get; set; }

        /// <summary>
        /// Gets or sets date of issue of the birth certificate.
        /// </summary>
        public DateTime SvidDate { get; set; }

        public virtual Child Child { get; set; }
    }
}
