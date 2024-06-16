using Vortice.XInput;

namespace ZangelGameSyncClient
{
    internal class ConsoleOptions
    {

        private static readonly List<GamepadButtons> choiceButtons = [GamepadButtons.X, GamepadButtons.Y, GamepadButtons.A, GamepadButtons.B];
        private static readonly List<string> choiceButtonsText = ["X", "Y", "A", "B"];
        private static bool gamepadIsPressed(GamepadButtons button)
        {
            bool pressed = false;
            for (int i = 0; i < 4; i++)
            {
                if (XInput.GetState(0, out State state))
                {
                    if ((state.Gamepad.Buttons & button) != 0)
                    {
                        pressed = true;
                        break;
                    }
                };
            }
            return pressed;
        }


        public static int ChoiceConfirm(string prompt, List<string> choices, List<string> choiceConfirmMsgs)
        {
            while (true)
            {
                int c = Choice(prompt, choices);

                if (YesNo(choiceConfirmMsgs[c], true))
                {
                    return c;
                }
            }
        }

        public static int Choice(string prompt, List<string> choices)
        {
            if (choices.Count > 4) { throw new NotImplementedException("Choice only support a max of 4 elements"); }

            ConsolePrinter.Info(prompt);

            // print options
            int optionCount = choices.Count;
            for (int i = 0; i < optionCount; i++)
            {
                ConsolePrinter.Info($"[{i + 1} / {choiceButtonsText[i]}] " + choices[i]);
            }

            int? choiceBreak = null;
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    char x = Console.ReadKey(true).KeyChar;
                    if (x == '1' || x == '2' || x == '3' || x == '4')
                    {
                        return int.Parse(x.ToString()) - 1;
                    }
                    continue;
                }


                for (int i = 0; i < optionCount; i++)
                {
                    if (gamepadIsPressed(choiceButtons[i]))
                    {
                        choiceBreak = i;
                    }
                }

                if (choiceBreak != null)
                    break;

                Thread.Sleep(50);
            }

            while (true)
            {
                if (gamepadIsPressed(GamepadButtons.A) || gamepadIsPressed(GamepadButtons.B) || gamepadIsPressed(GamepadButtons.X) || gamepadIsPressed(GamepadButtons.Y)) // wait until all released
                {
                    Thread.Sleep(50);
                    continue;
                }

                return choiceBreak ?? 0;
            }
        }

        public static bool YesNoConfirm(string prompt, string? msgYesConfirm = null, string? msgNoConfirm = null)
        {
            while (true)
            {
                bool initial = YesNo(prompt);
                if (initial && msgYesConfirm != null)
                {
                    // double prompt
                    if (YesNo(msgYesConfirm, true))
                    {
                        return true;
                    }
                    // else loop
                    continue;
                }

                if (!initial && msgNoConfirm != null)
                {
                    // double prompt
                    if (YesNo(msgNoConfirm, true))
                    {
                        return false;
                    }
                    continue;
                }

                return initial;
            }
        }

        public static bool YesNo(string prompt, bool warn = false)
        {
            if (warn)
                ConsolePrinter.Warn(prompt + " Confirm [Y/n] [A/B]?");
            else
                ConsolePrinter.Info(prompt + " Confirm [Y/n] [A/B]?");

            bool? breakResult = null;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    switch (Console.ReadKey(true).KeyChar)
                    {
                        case 'y':
                            return true;
                        case 'n':
                            return false;
                        default:
                            break;
                    }
                }

                if (gamepadIsPressed(GamepadButtons.A))
                {
                    breakResult = true;
                }

                if (gamepadIsPressed(GamepadButtons.B))
                {
                    breakResult = false;
                }

                if (breakResult != null)
                {
                    break;
                }

                Thread.Sleep(50);
            }

            // if it came here, it means it pressed via xbox button, wait till button is released
            while (true)
            {
                if (gamepadIsPressed(GamepadButtons.A) || gamepadIsPressed(GamepadButtons.B))
                {
                    Thread.Sleep(50);
                    continue;
                }

                return breakResult ?? false;
            }
        }

        public static void AwaitInput()
        {
            ConsolePrinter.Info("Press any key/button to continue...");
            while (true)
            {
                if (Console.KeyAvailable) // await until any key pressed
                {
                    Console.ReadKey(true);
                    return;
                }

                // or any gamepad main button is pressed
                if (gamepadIsPressed(GamepadButtons.A) || gamepadIsPressed(GamepadButtons.B) || gamepadIsPressed(GamepadButtons.X) || gamepadIsPressed(GamepadButtons.Y))
                    return;

                Thread.Sleep(50);
            }
        }
    }
}
