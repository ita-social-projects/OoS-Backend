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
        /// Series of birth certificate.
        /// </summary>
        public string SvidSer { get; set; }

        /// <summary>
        /// Number of birth certificate.
        /// </summary>
        public string SvidNum { get; set; }

        public string SvidNumMD5 { get; set; }

        /// <summary>
        /// Authority that issued the birth certificate.
        /// </summary>
        public string SvidWho { get; set; }

        /// <summary>
        /// Date of issue of the birth certificate.
        /// </summary>
        public DateTime SvidDate { get; set; }
    }
}
