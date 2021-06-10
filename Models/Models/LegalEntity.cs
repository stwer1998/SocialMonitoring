using Models.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class LegalEntity
    {

        /// <summary>
        /// Идентификатор Юр. лица
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// ИНН юр. лице
        /// </summary>
        public string Inn { get; set; }

        /// <summary>
        /// Полное имя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Номер региона для ФССП
        /// </summary>
        [JsonIgnore]
        public virtual string RegionNum { get => Inn.ToString().Substring(0,2); }

        /// <summary>
        /// Сокрощенное имя
        /// </summary>
        [JsonIgnore]
        public virtual string ShortName { get => ErgulNalog.ShortName; }

        /// <summary>
        /// Данные с картотеки арбитражных дел
        /// </summary>
        public KadArbirtModel KadArbirt { get; set; }

        /// <summary>
        /// данные с ФССп
        /// </summary>
        public IList<FsspModel> FsspModels { get; set; }

        /// <summary>
        /// Данные с https://egrul.nalog.ru/index.html
        /// </summary>
        public ErgulNalogModel ErgulNalog { get; set; }

        /// <summary>
        /// Данные с https://bo.nalog.ru/
        /// </summary>
        public BoNalogModel BoNalogModel { get; set; }

        /// <summary>
        /// Состоил в реестре банкротство. True - состоит. False - не состоит 
        /// </summary>
        public bool OnBankruptcy { get; set; }

        /// <summary>
        /// Состоил в реестрt недобросовестных поставщиков. True - состоит. False - не состоит 
        /// </summary>
        public bool OnUnscrupulousVendors { get; set; }

        /// <summary>
        /// Для сохранение изменений
        /// </summary>
        [JsonIgnore]
        public List<DataHistory> DataHistories { get; set; }
        [JsonIgnore]
        public QueueState QueueState { get; set; }

        public string AnaliticsModuleResult { get; set; }

        public bool MassAddress { get; set; }

        public bool InTeroristList { get; set; }

        public override string ToString()
        {
            return $"{Inn} {ErgulNalog}";
        }

    }
}
