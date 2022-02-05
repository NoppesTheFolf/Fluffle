using System;
using System.Linq;

namespace Noppes.Fluffle.Bot.Routing
{
    public class InlineKeyboardTextAttribute : Attribute
    {
        public string Text { get; set; }

        public InlineKeyboardTextAttribute(string text)
        {
            Text = text;
        }
    }

    public static class EnumExtensions
    {
        public static string InlineKeyboardText<T>(this T value) where T : Enum
        {
            var memberInfos = typeof(T).GetMember(value.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == typeof(T));
            var valueAttributes = enumValueMemberInfo!.GetCustomAttributes(typeof(InlineKeyboardTextAttribute), false);

            return ((InlineKeyboardTextAttribute)valueAttributes[0]).Text;
        }
    }
}
