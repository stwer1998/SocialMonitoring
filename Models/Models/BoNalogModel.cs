using Newtonsoft.Json;

namespace Models
{
    public class BoNalogModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        /// <summary>
        /// Внеоборотные активы
        /// </summary>
        public double NonCurrentAssets { get; set; }
        
        /// <summary>
        /// Оборотные активы
        /// </summary>
        public double CurrentAssets { get; set; }
        
        /// <summary>
        /// Валюта баланса
        /// </summary>
        public double BalanceCurrency { get; set; }
        
        /// <summary>
        /// Уставной капитал
        /// </summary>
        public double AuthorizedCapital { get; set; }
        
        /// <summary>
        /// капитал и резервы
        /// </summary>
        public double CapitalAndReserves { get; set; }
        
        /// <summary>
        /// долгосрочные обязательства
        /// </summary>
        public double LongTermCommitment { get; set; }
        
        /// <summary>
        /// краткосрочные обязательства
        /// </summary>
        public double CurrentLiabilities { get; set; }
        
        /// <summary>
        /// выручка
        /// </summary>
        public double Revenue { get; set; }
        
        /// <summary>
        /// прибыль
        /// </summary>
        public double Profit { get; set; }
        
        /// <summary>
        /// заемные средства
        /// </summary>
        public double Borrowings { get; set; }
        
        /// <summary>
        /// кредиторская задолженность
        /// </summary>
        public double AccountsPayable { get; set; }
        
        /// <summary>
        /// Прибыль (убыток) от продаж
        /// </summary>
        public double ProfitFromSale { get; set; }
        
        /// <summary>
        /// Дейсвует с
        /// </summary>
        public string StartAction { get; set; }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BoNalogModel item))
            {
                return false;
            }
            else
            {
                return ToString() == item.ToString();
            }
        }

        public override string ToString()
        {
            return $"Внеоборотные активы:{NonCurrentAssets}; Оборотные активы:{CurrentAssets}; Валюта баланса:{BalanceCurrency}; Уставной капитал:{AuthorizedCapital}; капитал и резервы:{CapitalAndReserves}; " +
                $"долгосрочные обязательства:{LongTermCommitment}; краткосрочные обязательства:{CurrentLiabilities}; выручка:{Revenue}; прибыль:{Profit}; " +
                $"заемные средства:{Borrowings}; кредиторская задолженность:{AccountsPayable}; Прибыль (убыток) от продаж:{ProfitFromSale}; {StartAction}";
        }


    }
}
