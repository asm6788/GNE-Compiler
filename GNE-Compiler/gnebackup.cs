 class Operator
        {
            public readonly Type type;
            public readonly string Contents;

            public Operator(Type type, string contents)
            {
                this.type = type;
                Contents = contents;
            }

            static public List<Operator> Parse(string source)
            {
                bool Instring = false;
                bool startrec = true;
                string temp = "";
                List<Operator> operators = new List<Operator>();
                for (int i = 0; i != source.Length; i++) 
                {
                    if (source[i] == '"')
                    {
                        if(Instring)
                        {
                            temp += source[i+1];
                            operators.Add(new Operator(Type.String, temp));
                            temp = "";
                        }
                        Instring = !Instring;
                        continue;
                    }
                    else
                    {
                        if(i == source.Length-1)
                        {
                            AddParseList(ref startrec, ref temp, operators);
                            startrec = !startrec;
                            continue;
                        }
                        if (!Instring)
                        {
                            switch (source[i])
                            {
                                case '*':
                                    AddParseList(ref startrec, ref temp, operators);
                                    operators.Add(new Operator(Type.Multiplication, "*"));
                                    startrec = !startrec;
                                    continue;
                                case ';':
                                    AddParseList(ref startrec, ref temp, operators);
                                    startrec = !startrec;
                                    continue;
                                case '/':
                                    AddParseList(ref startrec, ref temp, operators);
                                    operators.Add(new Operator(Type.Division, "/"));
                                    startrec = !startrec;
                                    continue;
                                case '+':
                                    AddParseList(ref startrec, ref temp, operators);
                                    operators.Add(new Operator(Type.Add, "+"));
                                    startrec = !startrec;
                                    continue;
                                    //case '-':
                                    //    AddParseList(ref startrec, ref temp, operators);
                                    //    operators.Add(new Operator(Type.Minus, "-"));
                                    //    startrec = !startrec;
                                    //    continue;
                            }
                        }
                    }
                    if (startrec)
                    {
                        temp += source[i];
                    }

                }

                return operators;
            }

            private static void AddParseList(ref bool startrec, ref string temp, List<Operator> operators)
            {
                double number = 0;
                if (temp != "")
                {
                    temp = temp.Trim();
                    if(temp[temp.Length-1] == ')' && temp[temp.Length-2] == ')')
                    {
                        operators.Add(new Operator(Type.MasterFunction, temp.Remove(temp.IndexOf('('))));
                        string twice =  temp.Remove(0, temp.IndexOf('(') + 1);
                        twice = twice.Remove(twice.Length - 1);
                        operators.Add(new Operator(Type.SlaveFunction, twice));
                        startrec = false;
                    }
                    else if (temp.Contains("("))
                    {
                        operators.Add(new Operator(Type.Function, temp));
                        startrec = false;
                    }
                    else if (double.TryParse(temp, out number))
                    {
                        operators.Add(new Operator(Type.Number, temp));
                        startrec = false;
                    }
                    else if (!temp.Contains("("))
                    {
                        operators.Add(new Operator(Type.Variable, temp));
                        startrec = false;
                    }

                    temp = "";
                }
            }

            public enum Type
            {
                Add,
                Minus,
                Subtraction,
                Division,
                Multiplication,
                Modulo,
                String,
                Number,
                Variable,
                MasterFunction,
                SlaveFunction,
                Function
            }
        }