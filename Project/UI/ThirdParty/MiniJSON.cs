// Minimalny parser JSON (MIT) – skrócona wersja do słowników/list/number/string/bool/null
// Źródło bazowe: https://gist.github.com/darktable/1411710 (adaptacja pod Unity)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MiniJSON
{
    public static class Json
    {
        public static object Deserialize(string json) { if (json == null) return null; return Parser.Parse(json); }

        private sealed class Parser : IDisposable
        {
            private readonly string json; private int index;
            private Parser(string json){ this.json=json; }
            public static object Parse(string json){ using var p = new Parser(json); return p.ParseValue(); }
            public void Dispose(){}

            private char Peek => index < json.Length ? json[index] : '\0';
            private char Next => index < json.Length ? json[index++] : '\0';
            private void EatWhitespace(){ while (char.IsWhiteSpace(Peek)) index++; }

            private object ParseValue()
            {
                EatWhitespace();
                char c = Peek;
                if (c=='"') return ParseString();
                if (c=='{' ) return ParseObject();
                if (c=='[' ) return ParseArray();
                if (c=='t'){ Eat("true"); return true; }
                if (c=='f'){ Eat("false"); return false; }
                if (c=='n'){ Eat("null"); return null; }
                return ParseNumber();
            }

            private void Eat(string s){ for(int i=0;i<s.Length;i++) if (Next!=s[i]) throw new Exception("bad json"); }
            private string ParseString()
            {
                var sb = new StringBuilder(); if (Next!='"') throw new Exception("bad string");
                while (true)
                {
                    if (index>=json.Length) throw new Exception("eos");
                    char c = Next;
                    if (c=='"') break;
                    if (c=='\\')
                    {
                        c = Next;
                        switch(c)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                var hex = new string(new[]{Next,Next,Next,Next});
                                sb.Append((char)Convert.ToInt32(hex,16));
                                break;
                        }
                    }
                    else sb.Append(c);
                }
                return sb.ToString();
            }

            private IDictionary ParseObject()
            {
                var dict = new Dictionary<string, object>(); if (Next!='{') throw new Exception("bad obj");
                while (true)
                {
                    EatWhitespace(); if (Peek=='}'){ Next; break; }
                    var key = ParseString(); EatWhitespace(); if (Next!=':') throw new Exception(":");
                    var val = ParseValue(); dict[key] = val; EatWhitespace();
                    if (Peek==','){ Next; continue; }
                    if (Peek=='}'){ Next; break; }
                }
                return dict;
            }

            private IList ParseArray()
            {
                var list = new List<object>(); if (Next!='[') throw new Exception("bad arr");
                while (true)
                {
                    EatWhitespace(); if (Peek==']'){ Next; break; }
                    var v = ParseValue(); list.Add(v); EatWhitespace();
                    if (Peek==','){ Next; continue; }
                    if (Peek==']'){ Next; break; }
                }
                return list;
            }

            private object ParseNumber()
            {
                int start=index;
                while ("-+0123456789.eE".IndexOf(Peek)>=0) index++;
                var s = json.Substring(start, index-start);
                if (s.IndexOf('.')>=0 || s.IndexOf('e')>=0 || s.IndexOf('E')>=0) return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
                return long.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}