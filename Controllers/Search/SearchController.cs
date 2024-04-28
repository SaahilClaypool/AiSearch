global using Microsoft.AspNetCore.Mvc;

namespace AiSearch;

public class SearchController(Converser converser) : Controller
{
    [Route("/search/new")]
    public ActionResult New([FromQuery] string search)
    {
        var id = converser.StartConversation(search);
        return RedirectToAction(nameof(Conversation), new { id });
    }

    [Route("/search/conversation")]
    public ActionResult Conversation(Guid id)
    {
        ConversationState conversation;
        try
        {
            conversation = converser.Converasation(id);
        }
        catch
        {
            return Redirect("/");
        }
        return View(conversation);
    }

    [Route("/search/reply")]
    public ActionResult Conversation(
        [FromQuery] Guid id,
        [FromQuery] string search
    )
    {
        converser.Reply(id, search);
        return RedirectToAction(nameof(Conversation), new { id });
    }
}
