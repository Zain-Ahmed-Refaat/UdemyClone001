using UdemyClone.Data;
using UdemyClone.Entities;

namespace UdemyClone.Services
{
    public class SubCategoryService : BaseRepository<SubCategory>
    {
        public SubCategoryService(ApplicationDbContext context) : base(context)
        {
        }

    }
}
