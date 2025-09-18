using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENROLLMENT_V3
{
    public enum MrzDocField
    {
        Complete = 0,
        LineFirst = 1,
        LineSecond = 2,
        LineThird = 3,
        DocCode = 4,
        DocIssuer = 5,
        DocNumber = 6,
        DocNumberChecksum = 7,
        DocNumberOptional = 8,
        DocNumberComposite = 9,
        DocNumberCompositeChecksum = 10,
        HolderBirthDate = 11,
        HolderBirthDateYear = 12,
        HolderBirthDateMonth = 13,
        HolderBirthDateDay = 14,
        HolderBirthDateChecksum = 15,
        HolderSex = 16,
        DocExpiryDate = 17,
        DocExpiryDateYear = 18,
        DocExpiryDateMonth = 19,
        DocExpiryDateDay = 20,
        DocExpiryDateChecksum = 21,
        HolderNationality = 22,
        Optional = 23,
        OptionalChecksum = 24,
        Composite = 25,
        CompositeChecksum = 26,
        HolderName = 27,
        HolderNamePrimary = 28,
        HolderNameSecondary = 29,
        HolderNameTertiary = 30,
        HolderAddress = 31,
        HolderDisambiguation = 32,
        HolderBirthDateYearFull = 33,
        HolderBirthDateFull = 34,
        IdlConfiguration = 35,
        IdlDiscretionary = 36,
        DocNumberOptionalChecksum = 37,
        HolderNumber = 38,
        HolderNumberChecksum = 39,
        DocAuthority = 40
    }
    class MRZ
    {
        public string Complete { get; set; }
        public string LineFirst { get; set; }
        public string LineSecond { get; set; }
        public string LineThird { get; set; }
        public string DocCode { get; set; }
        public string DocIssuer { get; set; }
        public string DocNumber { get; set; }
        public string DocNumberChecksum { get; set; }
        public string DocNumberOptional { get; set; }
        public string DocNumberComposite { get; set; }
        public string DocNumberCompositeChecksum { get; set; }
        public string HolderBirthDate { get; set; }
        public string HolderBirthDateYear { get; set; }
        public string HolderBirthDateMonth { get; set; }
        public string HolderBirthDateDay { get; set; }
        public string HolderBirthDateChecksum { get; set; }
        public string HolderSex { get; set; }
        public string DocExpiryDate { get; set; }
        public string DocExpiryDateYear { get; set; }
        public string DocExpiryDateMonth { get; set; }
        public string DocExpiryDateDay { get; set; }
        public string DocExpiryDateChecksum { get; set; }
        public string HolderNationality { get; set; }
        public string Optional { get; set; }
        public string OptionalChecksum { get; set; }
        public string Composite { get; set; }
        public string CompositeChecksum { get; set; }
        public string HolderName { get; set; }
        public string HolderNamePrimary { get; set; }
        public string HolderNameSecondary { get; set; }
        public string HolderNameTertiary { get; set; }
        public string HolderAddress { get; set; }
        public string HolderDisambiguation { get; set; }
        public string HolderBirthDateYearFull { get; set; }
        public string HolderBirthDateFull { get; set; }
        public string IdlConfiguration { get; set; }
        public string IdlDiscretionary { get; set; }
        public string DocNumberOptionalChecksum { get; set; }
        public string HolderNumber { get; set; }
        public string HolderNumberChecksum { get; set; }
        public string DocAuthority { get; set; }        
    }
}
