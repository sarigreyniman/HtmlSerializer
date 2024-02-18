using HtmlSerializer;
using System.Text.RegularExpressions;

var html = await Load("https://learn.malkabruk.co.il/");

static async Task<string> Load(string url) { 
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}

HtmlElement rootElement = Buildtree(html);
PrintTree(rootElement, 0);

static HtmlElement Buildtree(string html)
{
    var clearHtml = new Regex(@"[\r\n]+").Replace(html, "");
    var htmlLines = new Regex("<(.*?)>").Split(clearHtml).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

    int keywordIndex = htmlLines.FindIndex(s => Regex.IsMatch(s, @"^\s*html"));
    htmlLines.RemoveRange(0, keywordIndex + 1);

    HtmlElement rootElement = new HtmlElement { Name = "html", Children = new List<HtmlElement>(), Parent = null };
    HtmlElement currentElement = rootElement;
    int currentIndex = 0;

    while (currentIndex < htmlLines.Count)
    {
        string line = htmlLines[currentIndex];

        if (line.StartsWith("!--"))
        {
            while (!line.Contains("-->"))
            {
                currentIndex++;
                line = htmlLines[currentIndex];
            }
            continue;
        }

        if (line == "/html")
        {
            break;
        }

        var attributes = new Regex("([^\\s]*?)=\"(.*?)\"").Matches(line).Cast<Match>()
            .Select(match => match.Value).ToList();

        int firstSpace = line.IndexOf(' ');
        string name, id = null, cls = null;
        List<string> classes = null;

        if (firstSpace == -1)
        {
            name = line;
        }
        else
            name = line.Substring(0, line.IndexOf(" "));

        if (attributes.Count > 0)
        {
            id = attributes.FirstOrDefault(attribute => attribute.StartsWith("id"));
            if (id != null)
            {
                attributes.RemoveAll(attribute => attribute == id);
                id = id.Substring(4);
            }

            cls = attributes.FirstOrDefault(attribute => attribute.StartsWith("class"));
            if (cls != null)
            {
                attributes.RemoveAll(attribute => attribute == cls);
                cls = cls.Substring(7);
                classes = cls.Split(" ").ToList();
            }
        }

        if (line.StartsWith("/"))
        {
            if (currentElement.Parent != null)
            {
                currentElement = currentElement.Parent;
            }
            else
            {
               // אם currentElement.Parent הוא null, עשה כאן משהו כמו להדפיס הודעת שגיאה או להפסיק את הלולאה
            }
        }
        else if (!HtmlHelper.Instance.isHtmlTag(name))
        {
            currentElement.InnerHtml = (string)line;
        }
        else if (HtmlHelper.Instance.IsSelfClosingTag(name))
        {
            HtmlElement child = new HtmlElement
            {
                Name = name,
                Id = id,
                Classes = classes,
                Attributes = attributes,
                Parent = currentElement
            };
            currentElement.Children.Add(child);
        }
        else
        {
            HtmlElement child = new HtmlElement
            {
                Name = name,
                Id = id,
                Classes = classes,
                Attributes = attributes,
                Parent = currentElement,
                Children = new List<HtmlElement>()
            };

            currentElement.Children.Add(child);
            currentElement = child;
        }
        currentIndex++;
    }
    return rootElement;
}

static void PrintTree(HtmlElement element, int depth)
{
    Console.WriteLine(new string(' ', depth * 2) + element.Name);

    foreach (var child in element.Children)
    {
        PrintTree(child, depth + 1);
    }
}

string query = "div#content .site-content";
//Selector result = Selector.FromQueryString(query);
Selector selectorObject = Selector.FromQueryString(query);
Printtt(selectorObject);

void Printtt(Selector selector)
{
    if (selector != null)
    {
        Console.Write(selector.ToString());

        if (selector.Child != null)
        {
            Console.Write(" > ");
            Printtt(selector.Child);
        }
        else
        {
            Console.WriteLine(); // לשורה חדשה כשאין ילד
        }
    }
}
