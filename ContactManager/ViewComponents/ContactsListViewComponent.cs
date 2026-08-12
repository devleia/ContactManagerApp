using Microsoft.AspNetCore.Mvc;

namespace ContactManager.ViewComponents
{
    public class ContactsListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}