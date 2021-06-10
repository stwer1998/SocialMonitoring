using System;

namespace Models
{
    public class DataHistory
    {
        public int Id { get; set; }

        /// <summary>
        /// старыя запись
        /// </summary>
        public string OldData { get; set; }

        /// <summary>
        /// новая запись
        /// </summary>
        public string NewData { get; set; }
        
        /// <summary>
        /// дата изменение
        /// </summary>
        public DateTime ChangedDate { get; set; }
    }
}
