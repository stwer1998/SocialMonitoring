using Models;
using Models.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    public class CheckData : IParser
    {
        ApplicationContext db;
        public CheckData()
        {
            db = new ApplicationContext();
        }
        public async Task<LegalEntity> Parse(LegalEntity legalEntity)
        {
            CheckDbData(legalEntity);
            return legalEntity;
        }

        public async Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson)
        {
            CheckDbData(physicalPerson);
            return physicalPerson;
        }

        public async Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities)
        {
            foreach (var item in legalEntities)
            {
                CheckDbData(item);

            }
            return legalEntities;
        }

        public async Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople)
        {
            foreach (var item in physicalPeople)
            {
                CheckDbData(item);

            }
            return physicalPeople;
        }

        private void CheckDbData(LegalEntity legalEntity)
        {
            //check Director

            try
            {
                var regionName = GetText(@",([^,]*),\s?Г\.", legalEntity.ErgulNalog.FullAddress).ToUpper().Replace("ОБЛАСТЬ", string.Empty).Replace("РЕСПУБЛИКА", string.Empty);
                var city = GetText(@"Г\.([^,]*),", legalEntity.ErgulNalog.FullAddress);
                var street = GetText(@"УЛ\.([^,]*),", legalEntity.ErgulNalog.FullAddress);
                var homeNumber = GetText(@"Д\.([^,]*)", legalEntity.ErgulNalog.FullAddress);

                if (db.MassAddresses.FirstOrDefault(x => x.City.Contains(city) && x.StreetName.Contains(street) && x.HomeNumber == homeNumber) != null)
                {
                    legalEntity.MassAddress = true;
                }

                if (db.TerosistLegals.FirstOrDefault(x => x.Inn == legalEntity.Inn) != null)
                {
                    legalEntity.InTeroristList = true;
                }
            }
            catch(Exception ex)
            {

            }


        }

        private void CheckDbData(PhysicalPerson physicalPerson)
        {
            try
            {
                if (db.MassDirectors.FirstOrDefault(x => x.Inn == physicalPerson.Inn) != null)
                {
                    physicalPerson.MassDirector = true;
                }

                if (db.MassOwners.FirstOrDefault(x => x.Inn == physicalPerson.Inn) != null)
                {
                    physicalPerson.MassOwner = true;
                }

                if (db.Terosists.FirstOrDefault(x => x.BithDay == physicalPerson.BithDay && x.Name == $"{physicalPerson.LastName} {physicalPerson.Name} {physicalPerson.MiddleName}") != null)
                {
                    physicalPerson.InTeroristList = true;
                }
            }
            catch(Exception ex)
            {

            }

        }

        private string GetText(string reg, string text)
        {
            var res = Regex.Match(text, reg).Groups[1].Value;
            if (string.IsNullOrEmpty(res))
            {
                return string.Empty;
            }
            else
            {
                return res;
            }
        }
    }
}
