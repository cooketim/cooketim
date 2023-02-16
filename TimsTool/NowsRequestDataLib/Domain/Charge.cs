using System;
using System.Collections.Generic;
using System.Linq;

namespace NowsRequestDataLib.Domain
{
    public class Charge
    {
        public string Legislation;
        public string Title;
        public string Wording;
        public string ModeOfTrial;
        public string Code;

        public Charge(string legislation, string title, string modeOfTrial, string code, string wording, DateTime? startDateOfOffence, DateTime? endDateOfOffence)
        {
            Legislation = legislation;
            Title = title;
            ModeOfTrial = modeOfTrial;
            Code = code;
            Wording = string.Format(wording, startDateOfOffence, endDateOfOffence);
        }

        public static List<Charge> GetAllCharges(DateTime? startDateOfOffence, DateTime? endDateOfOffence)
        {
            var res = GetIndictableOnlyOrEitherWayCharges(startDateOfOffence, endDateOfOffence);
            res.AddRange(GetSummaryCharges(startDateOfOffence, endDateOfOffence));
            return res;
        }

        public static Charge GetMOJCharge(DateTime? startDateOfOffence, DateTime? endDateOfOffence, bool isLinkedApplication, bool isLiveProceedings)
        {
            if (!isLinkedApplication)
            {
                //stand alone application - taxi appeal
                return new Charge("In accordance with Section 61 of the Local Government (Miscellaneous Provisions) Act 1976.",
                    "An example of stand alone application, could become a civil case rather than stand alone application... Appeal against decision to suspend/revoke/refuse to renew a private hire vehicle operator's licence", null, "LM76505",
                    "Details of taxi appeal being made, could be another type of application for stand alone",
                    startDateOfOffence, endDateOfOffence);
            }

            if (isLiveProceedings)
            {
                //bail application
                return new Charge("In accordance with section 4 of the Bail Act 1976.",
                    "An example of live proceedings application... Application for bail following committal/sending/transfer in custody to appear at Crown Court", null, "BA76511",
                    "Details of bail application being made, could be another type of application for live proceedings",
                    startDateOfOffence, endDateOfOffence);
            }

            //failure to comply to requirements of a community order
            return new Charge("In accordance with Part 2 of Schedule 8 to the Criminal Justice Act 2003.",
                "An example of concluded proceedings application... Failing to comply with the requirements of a community order", null, "CJ03510",
                "Details of allegation of breach, etc.  Including Between {0} and {1}.  Could be another type of application for concluded proceedings e.g. appeal, variation, etc",
                startDateOfOffence, endDateOfOffence);
        }

        public static List<Charge> GetIndictableOnlyOrEitherWayCharges(DateTime? startDateOfOffence, DateTime? endDateOfOffence)
        {
            var res = new List<Charge>();
            res.Add(new Charge("Contrary to section 29(1)(b) and(2) of the Crime and Disorder Act 1998.", 
                "Racially / religiously aggravated wounding / grievous bodily harm.", "Indictable", "HC20008",
                "On {0} at Euston Train Station, London used racially threatening or abusive or insulting words or behaviour causing fear of or provoking violence.  Used a weapon for common purpose; and demonstrating toward the victim based on the victim's membership or presumed membership of a religious group.", 
                startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 29(1)(b) and(2) of the Crime and Disorder Act 1998.", 
                "Racially / religiously aggravated assault ocasioning actual bodily harm.", "Indictable", "HC20009",
                "On {0} at Euston Train Station, London used racially threatening or abusive or insulting words or behaviour causing fear of or provoking violence.  Used a weapon for common purpose; and demonstrating toward the victim based on the victim's membership or presumed membership of a religious group.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 5(3) of and Schedule 4 to the Misuse of Drugs Act 1971.", 
                "Possess with intent to supply a controlled drug of Class B - Cannabis", "Either Way", "HC20010",
                "On the {0} had in his possession a quantity of cannabis, a controlled drug of Class B with intent to supply it to another in contravention of section 4(1) of the Misuse of Drugs Act 1971.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 13 of the Theft Act 1968.",
                "Abstract / use without authority electricity", "Indictable", "HC20030",
                "Between {0} and {1} at 14 Brampton Road, Hornsey dishonestly used without due authority a quantity of electricity 585MW Contrary to section 13 of the Theft Act 1968.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 4(2)(a) of and Schedule 4 to the Misuse of Drugs Act 1971.",
                "Produce controlled drug of Class B - cannabis", "Indictable", "HC20031",
                "Between {0} and {1} at 19 Hillsea Street, Westbourne produced a quantity of cannabis, a controlled drug of class B, in contravention of section 4(1) of the Misuse of Drugs Act 1971 Contrary to section 4(2)(a) of and Schedule 4 to the Misuse of Drugs Act 1971.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 51(1) and(6) of the Criminal Justice and Public Order Act 1994.",
                "Intimidate a witness / juror", "Indictable", "HC20032",
                "Between {0} and {1} knowing or believing that Graham Swann was a witness in proceedings for an offence, did an act, namely contacting Graham Swann and following him to his home address which intimidated, and was intended to intimidate[insert name], intending thereby to cause the course of justice to be obstructed, perverted or interfered with.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 42A(1) and (5) of the Family Law Act 1996", "Breach of a non-molestation order", "Either Way", "HC20033", "On {0} without reasonable excuse did an act, namely contact by telephone, which you were prohibited from doing by a non - molestation order made by the Family Court at Luton Magistrates' Court", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 139(1) and (6) of the Criminal Justice Act 1988", "Possess knife blade / sharp pointed article in a public place", "Either Way", "HC20034", "On {0} at Clarence Park, St Albans you had with you, without good reason or lawful authority, in a public place the Clarence Park, St Albans an article which had a blade or was sharply pointed, namely Gerber Bear Grylls Machete.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 2 of the Road Traffic Act 1988 and Schedule 2 to the Road Traffic Offenders Act 1988", "Drive a motor vehicle dangerously", "Either Way", "HC20035", "On {0} at Chelmsford, Essex drove a mechanically propelled vehicle, namely BMW i8 registration number MX18 GLB dangerously on roads, namely A12 Southbound and A32 Westbound.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 170(2), (3) and (4) of and Schedule 1 to the Customs and Excise Management Act 1979", "Concerned in the fraudulent evasion of a prohibition on the importation of a class B drug", "Either Way", "HC20036", "On {0} at Northwick Green, Luton, in relation to a class 'B' drug, namely Cannabis, was knowingly concerned in the prohibition on importation thereof imposed by section 3(1) of the Misuse of Drugs Act 1971", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 5A(1)(a) and (2) of the Road Traffic Act 1988 and Schedule 2 to the Road Traffic Offenders Act 1988.", "Drive motor vehicle with a proportion of a specified controlled drug above the specified limit", "Either Way", "HC20037", "On {0} drove a motor vehicle, namely a BMW i8 MX18 GLB, on a road, namely A12 Southbound, when the proportion of a controlled drug, namely alcohol, in your blood, namely 258 micrograms per litre, exceeded the specified limit.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 20(1) and (2) of the Criminal Justice and Courts Act 2015", "Care worker ill-treat / wilfully neglect an individual", "Either Way", "HC20038", "On {0} at 18 Beaumont Crescent you having the care of Gregory Hill by virtue of being a care worker, ill-treated or wilfully neglected him", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 19 of and Schedule 6 to the Firearms Act 1968.", "Possess an imitation firearm in a public place", "Either Way", "HC20039", "On {0} at Highfields Cescent, without lawful authority or reasonable excuse, had with you in a public place, namely driveway of 18 Highfields Cescent, St Albans, an imitation firearm, namely Kalashnikov AK47 rifle", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 9(1)(a) of the Theft Act 1968.", "Burglary dwelling – with intent to steal", "Either Way", "HC20040", "On {0} at Sunderland Avenue, St Albans you entered as a trespasser a dwelling, namely 18 Sunderland Avenue, with intent to steal therein.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 18 of the Offences Against the Person Act 1861.", "Section 18 - wounding with intent", "Either Way", "HC20041", "On the {0} unlawfully and maliciously wounded James Wilson with intent to do him grievous bodily harm.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 8(1) of the Theft Act 1968.", "Robbery", "Either Way", "HC20042", "On {0} at 47 Bishop Close robbed Alyssa Hill of her Mangalsutra.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 1(1) of the Criminal Law Act 1977.", "Conspire to acquire / use / possess criminal property", "Either Way", "HC20043", "On {0} at 65 Nelson Street acquired, used or had possession of criminal property, namely 200 Berber Moroccan Rugs.", startDateOfOffence, endDateOfOffence));
            return res;
        }

        public static List<Charge> GetSummaryCharges(DateTime? startDateOfOffence, DateTime? endDateOfOffence)
        {
            var res = new List<Charge>();
            res.Add(new Charge("Contrary to section 5(1) and(6) of the Public Order Act 1986.", "Use threatening / abusive words / behaviour or disorderly behaviour likely to cause harassment, alarm or distress.", "Summary", "HC20016", "On {0} at Euston Train Station, London used threatening or abusive words or behaviour within the hearing or sight of a person likely to be caused harassment, alrm or distress thereby.", startDateOfOffence, endDateOfOffence));
            res.Add(new Charge("Contrary to section 8(1) of the Theft Act 1968.", "Stealing from the person of another.", "Summary", "HC20006", "On {0} at Euston Train Station, London robbed Penny Guerrero of a IPhone7 mobile telephone of a value unknown", startDateOfOffence, endDateOfOffence));
            return res;
        }
    }
}
