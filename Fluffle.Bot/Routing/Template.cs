using Stubble.Compilation.Builders;
using Stubble.Compilation.Interfaces;
using Stubble.Compilation.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace Noppes.Fluffle.Bot.Routing;

public static class Template
{
    private static IDictionary<string, dynamic> _renderFunctions;

    public static async Task CompileAsync()
    {
        var renderer = new StubbleCompilationBuilder().Configure(settings =>
        {
            settings.SetCompilationSettings(new CompilationSettings
            {
                ThrowOnDataMiss = true
            });
        }).Build();

        var method = typeof(IStubbleCompilationRenderer).GetMethods()
            .Where(m => m.Name == nameof(IStubbleCompilationRenderer.Compile))
            .Where(m => m.IsGenericMethod && m.GetGenericArguments().Length == 1)
            .Where(m => m.GetParameters().Length == 1)
            .First(m => m.GetParameters()[0].ParameterType == typeof(string));

        var templates = Directory
            .EnumerateFiles("Templates", "*.mustache", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension);

        var templateMethods = typeof(Template).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name != nameof(CompileAsync) && m.Name != nameof(Render))
            .Select(m => new
            {
                Template = m.Name,
                Model = m.GetParameters().Any() ? m.GetParameters()[0].ParameterType : null
            })
            .ToList();

        _renderFunctions = new Dictionary<string, dynamic>();
        foreach (var templateMethod in templateMethods)
        {
            var templateString = await File.ReadAllTextAsync(templates[templateMethod.Template]);
            if (templateMethod.Model == null)
            {
                string Get() => templateString;
                _renderFunctions.Add(templateMethod.Template, (Func<string>)Get);
                continue;
            }

            var compileMethod = method.MakeGenericMethod(templateMethod.Model);
            var renderFunction = compileMethod.Invoke(renderer, new object[] { templateString });
            _renderFunctions.Add(templateMethod.Template, renderFunction);
        }
    }

    private static string Render<T>(string template, T model) where T : class => ((Func<T, string>)_renderFunctions[template])(model);

    private static string Render(string template) => ((Func<string>)_renderFunctions[template])();

    public static string Start() => Render(nameof(Start));

    public static string Help(Message message) => Render(nameof(Help), message);

    public static string IHasFoundBug(Message message) => Render(nameof(IHasFoundBug), message);
}
