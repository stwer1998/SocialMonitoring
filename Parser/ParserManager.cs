using Microsoft.EntityFrameworkCore;
using Models;
using Models.DAL;
using Models.Dto;
using Models.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parser
{
    public class ParserManager
    {
        ApplicationContext db;
        public ParserManager()
        {
            db = new ApplicationContext();
        }

        public async void Update()
        {
            var legals = db.LegalEntities.Where(x => x.UpdatedTime < DateTime.Now.AddDays(-7)).ToList();
            await Parser.Parse(legals);
            db.LegalEntities.UpdateRange(legals);


            var physical = db.PhyicalPeople.Where(x => x.UpdatedTime < DateTime.Now.AddDays(-7)).ToList();
            await Parser.Parse(physical);
            db.PhyicalPeople.UpdateRange(physical);
        }

        public PhysicalPerson GetPhysicalOrParse(string inn)
        {
            if (string.IsNullOrEmpty(inn) && inn.Length != 12)
                return null;

            var data = db.PhyicalPeople
                .Include(x => x.FsspModels)
                .Include(x => x.KadArbirt)
                .FirstOrDefault(x => x.Inn == inn);

            if (data == null)
            {
                Task.Run(() => PhysicalStart(inn));
                return null;
            }
            else
            {
                return data;
            }
        }

        public PhysicalPerson GetPhysicalOrParse(PhysicalPerson physicalPerson)
        {
            var data = db.PhyicalPeople
                .Include(x => x.FsspModels)
                .Include(x => x.KadArbirt)
                .FirstOrDefault(x => x.Id == physicalPerson.Id);
            if (data != null)
            {
                data.VkName = physicalPerson.VkName;
                data.BithDay = physicalPerson.BithDay;
                data.Name = physicalPerson.Name;
                data.MiddleName = physicalPerson.MiddleName;
                data.LastName = physicalPerson.LastName;
                db.PhyicalPeople.Update(data);
                db.SaveChanges();
            }
            else
            {
                db.PhyicalPeople.Add(physicalPerson);
                db.SaveChanges();
            }
            Task.Run(() => PhysicalStart(physicalPerson));
            return data;
        }

        public LegalEntity GetLegalOrParse(string inn)
        {
            if (string.IsNullOrEmpty(inn) && inn.Length != 10)
                return null;

            var data = db.LegalEntities.Include(x=>x.BoNalogModel)
                .Include(x=>x.FsspModels)
                .Include(x=>x.KadArbirt)
                .Include(x=>x.ErgulNalog)
                .ThenInclude(x=>x.Owners)
                .Include(x=>x.ErgulNalog)
                .ThenInclude(x=>x.Director)
                .FirstOrDefault(x => x.Inn == inn);

            if (data == null)
            {
                Task.Run(() => LegalStart(inn));
                return null;
            }
            else
            {
                return data;
            }

        }

        public void UnSubscribe(string inn, string login) 
        {
            var user = db.Users.Include(x => x.Subscriptions)
                                .ThenInclude(x => x.LegalEntity)
                                .Include(x => x.Subscriptions)
                                .ThenInclude(x => x.PhyicalPerson)
                                .FirstOrDefault(x => x.Login == login);
            if (inn.Length == 10)
            {
                var data = user.Subscriptions.Where(x=>x.LegalEntity!=null).FirstOrDefault(x => x.LegalEntity.Inn == inn);
                if (data != null)
                {
                    var legal = db.LegalEntities.FirstOrDefault(x => x.Inn == inn);
                    user.Subscriptions.Remove(data);
                }
            }
            if (inn.Length == 12)
            {
                var data = user.Subscriptions.Where(x=>x.PhyicalPerson!=null).FirstOrDefault(x => x.PhyicalPerson.Inn == inn);
                if (data != null)
                {
                    var physical = db.PhyicalPeople.FirstOrDefault(x => x.Inn == inn);
                    user.Subscriptions.Remove(data);
                }
            }
            db.Users.Update(user);
            db.SaveChanges();
        }

        public void Subscribe(string inn, string login)
        {
            var user = db.Users.Include(x=>x.Subscriptions)
                                .ThenInclude(x=>x.LegalEntity)
                                .Include(x=>x.Subscriptions)
                                .ThenInclude(x=>x.PhyicalPerson)
                                .FirstOrDefault(x => x.Login == login);
            if (inn.Length == 10) 
            {
                var data = user.Subscriptions.Where(x=>x.LegalEntity!=null).FirstOrDefault(x => x.LegalEntity.Inn == inn);
                if (data == null)
                {
                    var legal = db.LegalEntities.FirstOrDefault(x => x.Inn == inn);
                    user.Subscriptions.Add(new Subscription { DateTime = DateTime.Now, LegalEntity = legal });
                }    
            }
            if (inn.Length == 12)
            {
                var data = user.Subscriptions.Where(x=>x.PhyicalPerson!=null).FirstOrDefault(x => x.PhyicalPerson.Inn == inn);
                if (data == null)
                {
                    var physical = db.PhyicalPeople.FirstOrDefault(x => x.Inn == inn);
                    user.Subscriptions.Add(new Subscription { DateTime = DateTime.Now, PhyicalPerson = physical });
                }
            }
            db.Users.Update(user);
            db.SaveChanges();
        }

        public User Monitoring(string login)
        {
            var user = db.Users.Include(x => x.Subscriptions)
                                .ThenInclude(x => x.LegalEntity)
                                .ThenInclude(x=>x.DataHistories)
                                .Include(x => x.Subscriptions)
                                .ThenInclude(x => x.PhyicalPerson)
                                .ThenInclude(x => x.DataHistories)
                                .FirstOrDefault(x => x.Login == login);
            return user;
        }

        private async void LegalStart(string inn) 
        {
            var legal = new LegalEntity { Inn = inn };
            var before = JsonConvert.SerializeObject(legal);

            var data = await Parser.Parse(legal);
            //var list = data?.ErgulNalog?.Owners.Select(x => new PhysicalPerson(x)).ToList();
            //list.Add(new PhysicalPerson(data.ErgulNalog.Director));
            var after = JsonConvert.SerializeObject(data);
            if (before != after)
            {
                if (data.DataHistories == null)
                {
                    data.DataHistories = new List<DataHistory>();
                }
                data.DataHistories.Add(new DataHistory { ChangedDate = DateTime.Now, OldData = before, NewData = after });
            }
            db.LegalEntities.Add(data);
            db.PhyicalPeople.AddRange();
            await db.SaveChangesAsync();
        }

        private async void PhysicalStart(PhysicalPerson physicalPerson)
        {
            var before = JsonConvert.SerializeObject(physicalPerson);
            var data = await Parser.Parse(physicalPerson);
            var after = JsonConvert.SerializeObject(data);
            if (before != after)
            {
                if (data.DataHistories == null)
                {
                    data.DataHistories = new List<DataHistory>();
                }
                data.DataHistories.Add(new DataHistory { ChangedDate = DateTime.Now, OldData = before, NewData = after });
            }
            db.PhyicalPeople.Update(data);
            await db.SaveChangesAsync();
        }

        private async void PhysicalStart(string inn)
        {
            var data = await Parser.Parse(new PhysicalPerson() { Inn=inn});
            db.PhyicalPeople.Add(data);
            await db.SaveChangesAsync();
        }

        public IEnumerable<ParserSetting> GetParserSettings() 
        {
            return db.ParserSettings.ToList();
        }

        public ParserSettingDto GetParserSetting(string name)
        {
            return new ParserSettingDto(db.ParserSettings.FirstOrDefault(x => x.ParserName == name));
        }
    }
}
