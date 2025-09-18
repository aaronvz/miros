using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.Text.RegularExpressions;

namespace ENROLLMENT_V3
{
    public class MrzData
    {
        public MrzData()
        {
            Format = MrzFormat.Unknown;
        }

        public MrzFormat Format { get; set; }
        public string DocumentType { get; internal set; }
        public string Country { get; internal set; }
        public string Hash { get; internal set; }
        public string DocumentNumber { get; internal set; }
        public string OptionalData1 { get; internal set; }
        public DateTime BirthDate { get; internal set; }
        public string Sex { get; internal set; }
        public DateTime ExpiryDate { get; internal set; }
        public string Nationality { get; internal set; }
        public string OptionalData2 { get; internal set; }
        public string SurName { get; internal set; }
        public string GivenName { get; internal set; }
        public string PersonalNumber { get; internal set; }
        public string OptionalData { get; internal set; }
    }

    public enum MrzFormat
    {
        Unknown,
        MachineReadableOfficialTravelDocuments1,
        MachineReadableOfficialTravelDocuments2,
        MachineReadableOfficialTravelDocuments3,
        MachineReadableVisasA,
        MachineReadableVisasB,
    }
    public static class MrzParser
    {
        private const string RegexAnyChar = "[A-Z0-9<]";
        private const string Regex09 = "[0-9]";
        private const string Regex09AngleBracket = "[0-9<]";
        private const string RegexAZ = "[A-Z]";
        private const string RegexSex = "[M|F|X|<]";

        private static readonly Func<int, string> RegexAnyCharGroupMatch = (int n) => $"({RegexAnyChar}{{{n}}})";
        private static readonly Func<int, string> Regex09GroupMatch = (int n) => $"({Regex09}{{{n}}})";
        private static readonly Func<int, string> Regex09AngleBracketGroupMatch = (int n) => $"({Regex09AngleBracket}{{{n}}})";
        private static readonly Func<int, string> RegexAZGroupMatch = (int n) => $"({RegexAZ}{{{n}}})";
        private static readonly Func<int, string> RegexSexGroupMatch = (int n) => $"({RegexSex}{{{n}}})";

        private static readonly string Td1Line0Regex = $"([A|C|I][A-Z0-9<]{{1}}){RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(9)}{Regex09GroupMatch(1)}{RegexAnyCharGroupMatch(15)}";
        private static readonly string Td1Line1Regex = $"{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexSexGroupMatch(1)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(11)}{Regex09GroupMatch(1)}";
        private static readonly string Td1Line2Regex = $"{RegexAnyCharGroupMatch(30)}";

        private static readonly string Td2Line0Regex = $"([A|C|I][A-Z0-9<]{{1}}){RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(31)}";
        private static readonly string Td2Line1Regex = $"{RegexAnyCharGroupMatch(9)}{Regex09GroupMatch(1)}{RegexAZGroupMatch(3)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexSexGroupMatch(1)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexAnyCharGroupMatch(7)}{Regex09GroupMatch(1)}";

        private static readonly string Td3Line0Regex = $"(P[A-Z0-9<]{{1}}){RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(39)}";
        private static readonly string Td3Line1Regex = $"{RegexAnyCharGroupMatch(9)}{Regex09GroupMatch(1)}{RegexAZGroupMatch(3)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexSexGroupMatch(1)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexAnyCharGroupMatch(14)}{Regex09AngleBracketGroupMatch(1)}{Regex09GroupMatch(1)}";

        private static readonly string MrvALine0Regex = $"(V[A-Z0-9<]{{1}}){RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(39)}";
        private static readonly string MrvALine1Regex = $"{RegexAnyCharGroupMatch(9)}{Regex09GroupMatch(1)}{RegexAZGroupMatch(3)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexSexGroupMatch(1)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexAnyCharGroupMatch(16)}";

        private static readonly string MrvBLine0Regex = $"(V[A-Z0-9<]{{1}}){RegexAZGroupMatch(3)}{RegexAnyCharGroupMatch(31)}";
        private static readonly string MrvBLine1Regex = $"{RegexAnyCharGroupMatch(9)}{Regex09GroupMatch(1)}{RegexAZGroupMatch(3)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexSexGroupMatch(1)}{Regex09GroupMatch(6)}{Regex09GroupMatch(1)}{RegexAnyCharGroupMatch(8)}";
        public static string error;

        public static MrzData Parse(List<string> lines)
        {
            error = string.Empty;
            var mrzData = new MrzData();
            var isSuccessfullyProcessed = false;
            try
            {
                var numberOfLines = lines.Count;
                if (numberOfLines != 2 && numberOfLines != 3)
                {
                    //Logger.Log($"{numberOfLines} not a valid number of lines. Expecting 2 or 3 lines");
                    error = $"{numberOfLines} not a valid number of lines. Expecting 2 or 3 lines";
                    return null;
                }
                var firstLine = lines[0];
                var firstLineLength = firstLine.Length;
                if (lines.Any(l => l.Length != firstLineLength))
                {
                    Logger.Log($"All lines must have same length:");
                    return null;
                }
                var format = MrzFormat.Unknown;
                if (numberOfLines == 3 && firstLineLength == 30)
                {
                    format = MrzFormat.MachineReadableOfficialTravelDocuments1;
                }
                else
                {
                    if (firstLineLength == 44 && numberOfLines == 2)
                    {
                        //pasaporte de 2 lineas
                        format = firstLine[0] == 'P' ? MrzFormat.MachineReadableOfficialTravelDocuments3 : MrzFormat.MachineReadableVisasA;
                    }
                    else
                    {
                        if (firstLineLength == 36 && numberOfLines == 2)
                        {
                            format = firstLine[0] == 'V' ? MrzFormat.MachineReadableVisasB : MrzFormat.MachineReadableOfficialTravelDocuments2;
                        }
                        else
                        {
                            format = MrzFormat.Unknown;
                        }
                    }
                }
                mrzData.Format = format;
                if (format == MrzFormat.Unknown)
                {
                    Logger.Log("Invalid type");
                    return null;
                }
                switch (format)
                {
                    case MrzFormat.MachineReadableOfficialTravelDocuments1:
                        isSuccessfullyProcessed = ProcessTravelDocument1(lines, ref mrzData);
                        break;
                    case MrzFormat.MachineReadableOfficialTravelDocuments2:
                        isSuccessfullyProcessed = ProcessTravelDocuments2(lines, ref mrzData);
                        break;
                    case MrzFormat.MachineReadableOfficialTravelDocuments3:
                        //pasaportes
                        isSuccessfullyProcessed = ProcessTravelDocuments3(lines, ref mrzData);
                        break;
                    case MrzFormat.MachineReadableVisasA:
                        isSuccessfullyProcessed = ProcessMachineReadableVisasA(lines, ref mrzData);
                        break;
                    case MrzFormat.MachineReadableVisasB:
                        isSuccessfullyProcessed = ProcessMachineReadableVisasB(lines, ref mrzData);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return null;
            }
            return isSuccessfullyProcessed ? mrzData : null;
        }

        private static bool ProcessTravelDocument1(List<string> lines, ref MrzData mrzData)
        {
            var isSuccessfullyProcessed = ProcessTravelDocument1Line0(lines[0], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessTravelDocument1Line1(lines[1], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessTravelDocument1Line2(lines[2], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            return true;
        }

        private static bool ProcessTravelDocument1Line0(string line, ref MrzData mrzData)
        {
            if(line.StartsWith("IDSLV"))
            {
                mrzData.DocumentType = "ID";
                mrzData.Country = "SLV";
                mrzData.DocumentNumber = line.Substring(5, line.Length - 5).Replace("<", "");
                mrzData.DocumentNumber = mrzData.DocumentNumber.Substring(0, mrzData.DocumentNumber.Length - 1);
                //mrzData.Hash = groups[4].Value; 
                mrzData.OptionalData1 = "";
            }
            else
            {
                var match = Regex.Match(line, Td1Line0Regex);
                if (!match.Success || match.Groups.Count != 6)
                {
                    Logger.Log("Invalid MRZ TD1, Line 0.");
                    return false;
                }
                var groups = match.Groups;
                mrzData.DocumentType = groups[1].Value;
                mrzData.Country = groups[2].Value;
                mrzData.DocumentNumber = groups[3].Value;
                //mrzData.Hash = groups[4].Value; 
                mrzData.OptionalData1 = groups[5].Value;
            }
            
            return true;
        }

        private static bool ProcessTravelDocument1Line1(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, Td1Line1Regex);
            if (!match.Success || match.Groups.Count != 9)
            {
                Logger.Log("Invalid MRZ TD1, Line 1.");
                return false;
            }
            var groups = match.Groups;
            mrzData.BirthDate = DateTime.ParseExact(groups[1].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[2].Value; 
            mrzData.Sex = groups[3].Value;
            mrzData.ExpiryDate = DateTime.ParseExact(groups[4].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[5].Value;
            mrzData.Nationality = groups[6].Value;
            mrzData.OptionalData2 = groups[7].Value;
            //mrzData.FinalHash = groups[8].Value; 
            return true;
        }

        private static bool ProcessTravelDocument1Line2(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, Td1Line2Regex);
            if (!match.Success || match.Groups.Count != 2)
            {
                Logger.Log("Invalid MRZ TD1, Line 2.");
                return false;
            }
            var groups = match.Groups;
            return ProcessNames(groups[1].Value, ref mrzData);
        }

        private static bool ProcessTravelDocuments2(List<string> lines, ref MrzData mrzData)
        {
            var isSuccessfullyProcessed = ProcessTravelDocument2Line0(lines[0], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessTravelDocument2Line1(lines[1], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            return true;
        }

        private static bool ProcessTravelDocument2Line0(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, Td2Line0Regex);
            if (!match.Success || match.Groups.Count != 4)
            {
                Logger.Log("Invalid MRZ TD2, Line 0.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentType = groups[1].Value;
            mrzData.Country = groups[2].Value;
            return ProcessNames(groups[3].Value, ref mrzData);
        }

        private static bool ProcessTravelDocument2Line1(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, Td2Line1Regex);
            if (!match.Success || match.Groups.Count != 11)
            {
                Logger.Log("Invalid MRZ TD2, Line 1.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentNumber = groups[1].Value;
            //mrzData.Hash = groups[2].Value; 
            mrzData.Nationality = groups[3].Value;
            mrzData.BirthDate = DateTime.ParseExact(groups[4].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[5].Value; 
            mrzData.Sex = groups[6].Value;
            mrzData.ExpiryDate = DateTime.ParseExact(groups[7].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[8].Value; 
            mrzData.OptionalData2 = groups[9].Value;
            //mrzData.FinalHash = groups[10].Value; 
            return true;
        }

        private static bool ProcessTravelDocuments3(List<string> lines, ref MrzData mrzData)
        {
            var isSuccessfullyProcessed = ProcessTravelDocument3Line0(lines[0], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessTravelDocument3Line1(lines[1], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            return true;
        }

        private static bool ProcessTravelDocument3Line0(string line, ref MrzData mrzData)
        {
            /**nombres  e icao **/
            var match = Regex.Match(line, Td3Line0Regex);
            if (!match.Success || match.Groups.Count != 4)
            {
                Logger.Log("Invalid MRZ TD3, Line 0.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentType = groups[1].Value;
            mrzData.Country = groups[2].Value;
            return ProcessNamesSplit(groups[3].Value, ref mrzData);
        }

        private static bool ProcessTravelDocument3Line1(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, Td3Line1Regex);
            if (!match.Success || match.Groups.Count != 12)
            {
                Logger.Log("Invalid MRZ TD3, Line 1.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentNumber = groups[1].Value;
            //mrzData.Hash = groups[2].Value; 
            mrzData.Nationality = groups[3].Value;
            mrzData.BirthDate = DateTime.ParseExact(groups[4].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[5].Value; 
            mrzData.Sex = groups[6].Value;
            mrzData.ExpiryDate = DateTime.ParseExact(groups[7].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[8].Value; 
            mrzData.PersonalNumber = groups[9].Value;
            //mrzData.Hash = groups[10].Value; 
            //mrzData.FinalHash = groups[11].Value; 
            return true;
        }

        private static bool ProcessMachineReadableVisasA(List<string> lines, ref MrzData mrzData)
        {
            var isSuccessfullyProcessed = ProcessMachineReadableVisasALine0(lines[0], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessMachineReadableVisasALine1(lines[1], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            return true;
        }

        private static bool ProcessMachineReadableVisasALine0(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, MrvALine0Regex);
            if (!match.Success || match.Groups.Count != 4)
            {
                Logger.Log("Invalid MRZ MRVA, Line 0.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentType = groups[1].Value;
            mrzData.Country = groups[2].Value;
            return ProcessNames(groups[3].Value, ref mrzData);
        }

        private static bool ProcessMachineReadableVisasALine1(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, MrvALine1Regex);
            if (!match.Success || match.Groups.Count != 10)
            {
                Logger.Log("Invalid MRZ MRVA, Line 1.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentNumber = groups[1].Value;
            //mrzData.Hash = groups[2].Value; 
            mrzData.Nationality = groups[3].Value;
            mrzData.BirthDate = DateTime.ParseExact(groups[4].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[5].Value; 
            mrzData.Sex = groups[6].Value;
            mrzData.ExpiryDate = DateTime.ParseExact(groups[7].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[8].Value; 
            mrzData.OptionalData = groups[9].Value;
            return true;
        }

        private static bool ProcessMachineReadableVisasB(List<string> lines, ref MrzData mrzData)
        {
            var isSuccessfullyProcessed = ProcessMachineReadableVisasBLine0(lines[0], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            isSuccessfullyProcessed = ProcessMachineReadableVisasBLine1(lines[1], ref mrzData);
            if (!isSuccessfullyProcessed)
            {
                return false;
            }
            return true;
        }

        private static bool ProcessMachineReadableVisasBLine0(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, MrvBLine0Regex);
            if (!match.Success || match.Groups.Count != 4)
            {
                Logger.Log("Invalid MRZ MRVB, Line 0.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentType = groups[1].Value;
            mrzData.Country = groups[2].Value;
            return ProcessNames(groups[3].Value, ref mrzData);
        }

        private static bool ProcessMachineReadableVisasBLine1(string line, ref MrzData mrzData)
        {
            var match = Regex.Match(line, MrvBLine1Regex);
            if (!match.Success || match.Groups.Count != 10)
            {
                Logger.Log("Invalid MRZ MRVB, Line 1.");
                return false;
            }
            var groups = match.Groups;
            mrzData.DocumentNumber = groups[1].Value;
            //mrzData.Hash = groups[2].Value; 
            mrzData.Nationality = groups[3].Value;
            mrzData.BirthDate = DateTime.ParseExact(groups[4].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[5].Value; 
            mrzData.Sex = groups[6].Value;
            mrzData.ExpiryDate = DateTime.ParseExact(groups[7].Value, "yyMMdd", CultureInfo.InvariantCulture);
            //mrzData.Hash = groups[8].Value; 
            mrzData.OptionalData = groups[9].Value;
            return true;
        }

        private static bool ProcessNames(string nameStr, ref MrzData mrzData)
            /*** prueba de nombres **/
        {
            if (string.IsNullOrWhiteSpace(nameStr))
            {
                return true;
            }
            var names = nameStr.Replace("<", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Any())
            {
                mrzData.SurName = names[0];
                mrzData.GivenName = string.Join(" ", names.Skip(1));
            }
            return true;
        }

        private static bool ProcessNamesSplit(string nameStr, ref MrzData mrzData)
        /*** prueba de nombres **/
        {
            if (string.IsNullOrWhiteSpace(nameStr))
            {
                return true;
            }
            //var names = nameStr.Replace("<", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] separatingStrings = { "<<" };
            string [] names= nameStr.Split(separatingStrings,StringSplitOptions.RemoveEmptyEntries);
            if (names.Any())
            {
                var surname = names[0].Replace("<", " ");
                var givename = names[1].Replace("<", " ");
                mrzData.SurName = surname;
                mrzData.GivenName = givename;
            }
            return true;
        }

        public static class Logger
        {
            public static void Log(string m)
            {
                error = m;
                Console.WriteLine(m);
            }
            public static void Log(Exception m)
            {
                error = m.Message;
                Console.WriteLine(m);
            }
        }
    }
}
