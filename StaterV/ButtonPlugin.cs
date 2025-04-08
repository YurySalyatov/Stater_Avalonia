using StaterV.PluginManager;

// TODO: practice NEED

namespace StaterV
{
    /// <summary>
    /// Плагин, которому из интерфейса нужна кнопка на тулбаре. По кнопке он запускается и возвращает результат.
    /// В качестве аргумента ему передается текущая система автоматов.
    /// </summary>
    public abstract class ButtonPlugin : BasePlugin
    {
        //public abstract PluginRetVal Start(List<StateMachine.StateMachine> _machines);
        public abstract PluginRetVal Start(PluginParams pluginParams);

        public abstract PluginRetVal SilentStart(PluginParams pluginParams);

        /// <summary>
        /// Проверка, можно ли использовать это имя в языках программирования.
        /// Check if we can use this name in common programming languages.
        /// </summary>
        /// <param name="name">Checking name</param>
        /// <returns></returns>
        public static bool IsNameAcceptable(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }

            if (!IsValidFirstChar(name[0]))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!IsValidMiddleChar(name[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsEngLetter(char ch)
        {
            if (ch >= 'a' && ch <= 'z')
            {
                return true;
            }

            if (ch >= 'A' && ch <= 'Z')
            {
                return true;
            }
            return false;
        }

        public static bool IsDigit(char ch)
        {
            return ch >= '0' && ch <= '9';
        }

        public static bool IsSpecSymbol(char ch)
        {
            return ch == '_';
        }

        public static bool IsValidFirstChar(char ch)
        {
            return (IsEngLetter(ch) || IsSpecSymbol(ch));
        }

        public static bool IsValidMiddleChar(char ch)
        {
            return (IsEngLetter(ch) || IsDigit(ch) || IsSpecSymbol(ch));
        }
    }
}
