using Models.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class PhysicalPerson
    {
        public PhysicalPerson()
        {
                
        }

        public PhysicalPerson(PhysicalLite physicalLite)
        {
            Inn = physicalLite.Inn;
            var names = physicalLite.Name.Split(' ');
            LastName = names[0].Trim();
            Name= names[1].Trim();
            MiddleName= names[2].Trim();
            QueueState = QueueState.NotFullInfoForParsing;
        }
        [JsonIgnore]
        public int Id { get; set; }

        public string Inn { get; set; }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public DateTime BithDay { get; set; }

        /// <summary>
        /// Номер региона для ФССП
        /// </summary>
        [JsonIgnore]
        public virtual string RegionNum { get => Inn.ToString().Substring(0, 2); }

        /// <summary>
        /// Данные с картотеки арбитражных дел
        /// </summary>
        public KadArbirtModel KadArbirt { get; set; }

        /// <summary>
        /// данные с ФССп
        /// </summary>
        public IList<FsspModel> FsspModels { get; set; }

        /// <summary>
        /// Состоил в реестре банкротство. True - состоит. False - не состоит 
        /// </summary>
        public bool OnBankruptcy { get; set; }

        /// <summary>
        /// Состоил в реестрt недобросовестных поставщиков. True - состоит. False - не состоит 
        /// </summary>
        public bool OnUnscrupulousVendors { get; set; }

        public string VkData { get;set; }

        public string VkName { get; set; }

        public bool MassOwner { get; set; }

        public bool MassDirector { get; set; }
        public DateTime UpdatedTime { get; set; }


        public bool InTeroristList { get; set; }
        [JsonIgnore]
        public QueueState QueueState { get; set; }

        /// <summary>
        /// Для сохранение изменений
        /// </summary>
        [JsonIgnore]
        public List<DataHistory> DataHistories { get; set; }

    }
}
