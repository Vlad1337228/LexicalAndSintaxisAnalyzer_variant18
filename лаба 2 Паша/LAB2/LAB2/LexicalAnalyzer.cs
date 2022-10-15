﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB2
{
    // Тип токена.
    enum TokenKind 
    {
      Number,     // Число.
      Identifier, // Идентификатор.
      ExclamationPoint, // Восклицательный знак
      Colon,      // Двоеточие
      LeftParen,  // (.
      RightParen, // ).
      EndOfText,  // Конец текста.
      Unknown     // Неизвестный.
    };

    // Класс "Токен".
    class Token
    {
        private string value;   // Значение токена (само слово).
        private TokenKind type; // Тип токена.

        // Позиция токена в исходном тексте.
        private int lineIndex;     // Индекс строки.
        private int symStartIndex; // Индекс символа в строке lineIndex, с которого начинается токен.

        // Значение токена (само слово).
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }

        // Тип токена.
        public TokenKind Type
        {
            get { return type; }
            set { this.type = value; }
        }

        // Индекс строки в исходном тексте, на которой расположен токен.
        public int LineIndex
        {
            get { return lineIndex; }
            set { this.lineIndex = value; }
        }

        // Индекс символа в строке LineIndex в исходном тексте, с которого начинается токен.
        public int SymStartIndex
        {
            get { return symStartIndex; }
            set { this.symStartIndex = value; }
        }

        // Сбросить значения полей токена.
        public void Reset()
        {
            this.value = "";
            this.type = TokenKind.Unknown;
            this.lineIndex = -1;
            this.symStartIndex = -1;
        }

        // Конструктор токена.
        public Token()
        {
            Reset(); // Сбрасываем значения полей токена.
        }
    }

    // Класс исключительных ситуаций лексического анализа.
    class LexAnException : Exception
    {
        // Позиция возникновения исключительной ситуации в анализируемом тексте.
        private int lineIndex; // Индекс строки.
        private int symIndex;  // Индекс символа.

        // Индекс строки, где возникла исключительная ситуация - свойство только для чтения.
        public int LineIndex
        {
            get { return lineIndex; }
        }

        // Индекс символа, на котором возникла исключительная ситуация - свойство только для чтения.
        public int SymIndex
        {
            get { return symIndex; }
        }

        // Конструктор исключительной ситуации.
        // message - описание исключительной ситуации.
        // lineIndex и symIndex - позиция возникновения исключительной ситуации в анализируемом тексте.
        public LexAnException(string message, int lineIndex, int symIndex) : base(message)
        {
            this.lineIndex = lineIndex;
            this.symIndex = symIndex;
        }    
    }

    // Класс "Лексический анализатор".
    // При обнаружении ошибки в исходном тексте он генерирует исключительную ситуацию LexAnException.
    class LexicalAnalyzer
    {
        // Тип символа.
        enum SymbolKind
        {
            Letter,    // Буква.
            Digit,     // Цифра.
            Space,     // Пробел.

            ExclamationPoint, // Восклицательный знак
            Colon,      // Двоеточие

            Reserved,  // Зарезервированный.
            Other,     // Другой.
            EndOfLine, // Конец строки.
            EndOfText  // Конец текста.
        };

        private const char commentSymbol1 = '/'; // Первый символ комментария.
        private const char commentSymbol2 = '*'; // Второй символ комментария.

        private string[] inputLines; // Входной текст - массив строк.
        private int curLineIndex;    // Индекс текущей строки.
        private int curSymIndex;     // Индекс текущего символа в текущей строке.

        private char curSym;           // Текущий символ.
        private SymbolKind curSymKind; // Тип текущего символа.

        private Token token; // Токен, распознанный при последнем вызове метода RecognizeNextToken().

        // Обработать лексическую ошибку.
        // msg - описание ошибки.
        private void LexicalError(string msg)
        {
            // Генерируем исключительную ситуацию, тем самым полностью прерывая процесс анализа текста.
            throw new LexAnException(msg, curLineIndex, curSymIndex);
        }

        
        // Считать следующий символ.
        private void ReadNextSymbol()
        {
            if (curLineIndex >= inputLines.Count()) // Если индекс текущей строки выходит за пределы текстового поля.
            {
                curSym = (char)0; // Обнуляем значение текущего символа.
                curSymKind = SymbolKind.EndOfText; // Тип текущего символа - конец текста.
                return;
            }

            curSymIndex++; // Увеличиваем индекс текущего символа.

            if (curSymIndex >= inputLines[curLineIndex].Count()) // Если индекс текущего символа выходит за пределы текущей строки.
            {
                curLineIndex++; // Увеличиваем индекс текущей строки.
                
                if (curLineIndex < inputLines.Count()) // Если индекс текущей строки находится в пределах текстового поля.
                {
                    curSym = (char)0; // Обнуляем значение текущего символа.
                    curSymIndex = -1; // Переносим индекс текущего символа в начало строки.
                    curSymKind = SymbolKind.EndOfLine; // Тип текущего символа - конец строки.
                    return;
                }
                else
                {
                    curSym = (char)0; // Обнуляем значение текущего символа.
                    curSymKind = SymbolKind.EndOfText; // Тип текущего символа - конец текста.
                    return;
                }
            }

            curSym = inputLines[curLineIndex][curSymIndex]; // Считываем текущий символ.

            ClassifyCurrentSymbol(); // Классифицируем текущий символ.
        }

        // Конструктор лексического анализатора. 
        // В качестве параметра передается исходный текст.
        public LexicalAnalyzer(string[] inputLines)
        {
            this.inputLines = inputLines;

            // Обнуляем поля.
            curLineIndex = 0;
            curSymIndex = -1;
            curSym = (char)0;
            token = null;

            // Считываем первый символ входного текста.
            ReadNextSymbol();
        }

        // Токен, распознанный при последнем вызове метода RecognizeNextToken() - свойство только для чтения.
        public Token Token 
        {
            get { return token; }
        }

        // Индекс текущей строки - свойство только для чтения.
        public int CurLineIndex
        {
            get { return curLineIndex; }
        }

        // Индекс текущего символа в текущей строке - свойство только для чтения.
        public int CurSymIndex
        {
            get { return curSymIndex; }
        }


        // Классифицировать текущий символ.
        private void ClassifyCurrentSymbol()
        {
            if (((int)curSym >= (int)'a') && ((int)curSym <= (int)'d')) // Если текущий символ лежит в диапазоне строчных латинских букв.
            {
                curSymKind = SymbolKind.Letter; // Тип текущего символа - буква.
            }
            else if (((int)curSym >= (int)'0') && ((int)curSym <= (int)'1')) // Если текущий символ лежит в диапазоне цифр.
            {
                curSymKind = SymbolKind.Digit; // Тип текущего символа - цифра.
            }
            else
            {
                switch (curSym)
                {
                    case ' ': // Если текущий символ - пробел.
                        curSymKind = SymbolKind.Space; // Тип текущего символа - пробел.
                        break;

                    // Если текущий символ - точка или первый символ комментария или второй символ комментария или символ подчеркивания,
                    
                    case commentSymbol1:
                    case commentSymbol2:
                    case '!':
                        //curSymKind = SymbolKind.ExclamationPoint; // Тип текущего символа - зарезервированный.
                        //break;
                    case ':':
                        //curSymKind = SymbolKind.Colon; // Тип текущего символа - зарезервированный.
                        //break;
                    case '(':
                    case ')':
                        curSymKind = SymbolKind.Reserved; // Тип текущего символа - зарезервированный.
                        break;

                    default:
                        curSymKind = SymbolKind.Other; // Тип текущего символа - другой.
                        break;
                }
            }
        }


        // Распознать идентификатор.
        private void RecognizeIdentifier()
        {
            goto A;

        A:
            if(curSymKind == SymbolKind.Letter)
            {
                token.Value += curSym; // Наращиваем значение текущего токена.
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto B; // Переходим в указанное состояние.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        B:  
            if(curSymKind == SymbolKind.EndOfText || curSymKind == SymbolKind.Space)
            {
                goto Fin;
            }

            if(curSymKind == SymbolKind.Letter)
            {
                token.Value += curSym; // Наращиваем значение текущего токена.
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto C; // Переходим в указанное состояние.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        C:
            if (curSymKind == SymbolKind.EndOfText || curSymKind == SymbolKind.Space)
            {
                goto Fin;
            }

            if (curSymKind == SymbolKind.Letter)
            {
                token.Value += curSym; // Наращиваем значение текущего токена.
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto D; // Переходим в указанное состояние.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        D:
            if (curSymKind == SymbolKind.EndOfText || curSymKind == SymbolKind.Space)
            {
                goto Fin;
            }

            if (curSymKind == SymbolKind.Letter)
            {
                token.Value += curSym; // Наращиваем значение текущего токена.
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto Fin; // Переходим в указанное состояние.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        Fin:
            token.Type = TokenKind.Identifier;
            return;


        }

        // Распознать число (целое или вещественное).
        private void RecognizeNumber()
        {
            goto A;

        A:
            if(curSymKind == SymbolKind.Digit)
            {
                
                token.Value += curSym; // Наращиваем значение текущего токена.
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto B; // Переходим в указанное состояние.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        B:
            if(curSymKind == SymbolKind.Digit)
            {
                if(curSym == '0')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto C; // Переходим в указанное состояние.
                }
                if(curSym == '1')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto G; // Переходим в указанное состояние.
                }
            }
            else if(curSymKind == SymbolKind.EndOfText || curSymKind == SymbolKind.Space || curSymKind == SymbolKind.Reserved)
            {
                goto Fin;
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        C:
            if (curSymKind == SymbolKind.Digit)
            {
                if (curSym == '0')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto B; // Переходим в указанное состояние.
                }
                if (curSym == '1')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto A; // Переходим в указанное состояние.
                }
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        G:
            if(curSymKind == SymbolKind.Digit)
            {
                if(curSym == '0')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto H; // Переходим в указанное состояние.
                }
                else
                    LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        H:
            if(curSymKind == SymbolKind.Digit)
            {
                if(curSym == '1')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto E; // Переходим в указанное состояние.
                }
                else
                    LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

        E:
            if(curSymKind == SymbolKind.Digit)
            {
                if(curSym == '1')
                {
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    goto G; // Переходим в указанное состояние.
                }
                else
                    LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.

            }
            else if(curSymKind == SymbolKind.EndOfText || curSymKind == SymbolKind.Space)
            {
                goto Fin;
            }
            else
                LexicalError("Недопустимый символ или незаконченный токен."); // Обнаружена ошибка в тексте.



        Fin:
            token.Type = TokenKind.Number;
            return;

        }

        // Распознать зарезервированный символ.
        private void RecognizeReservedSymbol()
        {
            switch (curSym)
            {
                case '(':
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    token.Type = TokenKind.LeftParen; // Тип распознанного токена - "(".
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    break;

                case ')':
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    token.Type = TokenKind.RightParen; // Тип распознанного токена - ")".
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    break;

                case '!':
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    token.Type = TokenKind.ExclamationPoint; // Тип распознанного токена - "!".
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    break;

                case ':':
                    token.Value += curSym; // Наращиваем значение текущего токена.
                    token.Type = TokenKind.Colon; // Тип распознанного токена - ":".
                    ReadNextSymbol(); // Читаем следующий символ в тексте.
                    break;

                default:
                    LexicalError("Неизвестный зарезервированный символ '" + curSym + "'"); // Обнаружена ошибка в тексте.
                    break;
            }


        }

        // Пропустить комментарий.
        private void SkipComment()
        {
            goto S; // Запускаем конечный автомат.

        // Конечный автомат для комментария.
        // S - начальное состояние.
        //----------------------------------------------------//
        S:
            if (curSym == commentSymbol1)
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto A; // Переходим в указанное состояние.
            }
            else
                LexicalError("Ожидалось " + commentSymbol1); // Обнаружена ошибка в тексте.

            A:
            if (curSym == commentSymbol2)
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto B; // Переходим в указанное состояние.
            }
            else
                LexicalError("Ожидалось " + commentSymbol2); // Обнаружена ошибка в тексте.

            B:
            if (curSym == commentSymbol2)
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto C; // Переходим в указанное состояние.
            }
            else if (curSymKind == SymbolKind.EndOfText)
                LexicalError("Незаконченный комментарий"); // Обнаружена ошибка в тексте.
            else
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto B; // Переходим в указанное состояние.
            }

        C:
            if (curSym == commentSymbol1)
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto Fin; // Переходим в указанное состояние.
            }
            else if (curSym == commentSymbol2)
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto C; // Переходим в указанное состояние.
            }
            else if (curSymKind == SymbolKind.EndOfText)
                LexicalError("Незаконченный комментарий"); // Обнаружена ошибка в тексте.
            else
            {
                ReadNextSymbol(); // Читаем следующий символ в тексте.
                goto B; // Переходим в указанное состояние.
            }

        Fin:
            goto Quit; // Выходим из конечного автомата.
                       //----------------------------------------------------//
                       // Конец конечного автомата для комментария.

        Quit:
            return;
        }


        // Распознать следующий токен в тексте.
        public void RecognizeNextToken()
        {
            // На данный момент уже прочитан символ, следующий за токеном, распознанным в прошлом вызове этого метода.
            // Если же это первый вызов, то на данный момент уже прочитан первый символ текста (в конструкторе).
            
            // Цикл пропуска пробелов, переходов на новую строку, комментариев.
            while ( (curSymKind == SymbolKind.Space) || 
                    (curSymKind == SymbolKind.EndOfLine) ||
                    (curSym == commentSymbol1) )
            {
                if (curSym == commentSymbol1) // Если текущий символ - первый символ комментария.
                    SkipComment(); // Пропускаем комментарий.
                else
                    ReadNextSymbol(); // Пропускаем пробел или переход на новую строку.
            }

            // Создаем новый экземпляр токена.
            token = new Token();

            // Запоминаем позицию начала токена в исходном тексте. 
            token.LineIndex = curLineIndex;
            token.SymStartIndex = curSymIndex;

            switch (curSymKind) // Анализируем текущий символ.
            {
                case SymbolKind.Letter: // Если текущий символ - буква.
                    RecognizeIdentifier(); // Вызываем процедуру распознавания идентификатора.            
                    break;

                case SymbolKind.Digit: // Если текущий символ - цифра.
                    RecognizeNumber(); // Вызываем процедуру распознавания числа (целого или вещественного).
                    break;

                case SymbolKind.Reserved: // Если текущий символ - зарезервированный.
                    RecognizeReservedSymbol(); // Вызываем процедуру распознавания зарезервированного символа.
                    break;

                case SymbolKind.EndOfText: // Если текущий символ - конец текста.
                    token.Type = TokenKind.EndOfText; // Тип распознанного токена - конец текста.
                    break;

                default: // Если текущий символ - какой-то другой.
                    LexicalError("Недопустимый символ"); // Обнаружена ошибка в тексте.
                    break;
            }
        }
    }
}
