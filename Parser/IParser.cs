using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parser
{
    public interface IParser
    {
        Task<LegalEntity> Parse(LegalEntity legalEntity);

        Task<PhysicalPerson> Parse(PhysicalPerson physicalPerson);

        Task<IEnumerable<LegalEntity>> Parse(IEnumerable<LegalEntity> legalEntities);

        Task<IEnumerable<PhysicalPerson>> Parse(IEnumerable<PhysicalPerson> physicalPeople);
    }
}
