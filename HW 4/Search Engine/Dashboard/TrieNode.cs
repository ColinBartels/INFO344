using System;
using System.Collections.Generic;
using System.Linq;

namespace Query_Suggestion
{
    public class TrieNode
    {
        private char c;
        private Dictionary<char, TrieNode> childrenDic;
        private List<string> childrenList;
        private bool endOfWord;

        public TrieNode(char c)
        {
            this.c = Char.ToLower(c);
            childrenDic = null;
            childrenList = null;
            endOfWord = false;
        }

        public char getChar()
        {
            return c;
        }

        public bool isTrie()
        {
            return childrenDic != null;
        }

        public List<TrieNode> getChildrenDic()
        {
            if (childrenDic != null)
            {
                return childrenDic.Values.ToList();
            }
            else
            {
                return null;
            }

        }

        public List<string> getChildrenList()
        {
            if (childrenList != null)
            {
                return childrenList;
            }
            else
            {
                return null;
            }
        }

        public void addChildDic(char input)
        {
            input = Char.ToLower(input);
            if (!childrenDic.ContainsKey(input))
            {
                childrenDic.Add(input, new TrieNode(input));
            }
        }

        public void addChildList(string line)
        {
            line = line.ToLower();
            if (childrenList == null)
            {
                childrenList = new List<string>();
            }
            if (line.Length > 0 && !childrenList.Contains(line))
            {
                childrenList.Add(line);
            }
            else if (line.Length == 0)
            {
                endOfWord = true;
            }
           
            if (childrenList.Count == 15)
            {
                convert();
            }
        }

        private void convert()
        {
            childrenDic = new Dictionary<char, TrieNode>();
            foreach (string line in childrenList)
            {
                char child = line[0];

                if (childrenDic.ContainsKey(child))
                {
                    childrenDic[child].addChildList(line.Substring(1));
                }
                else
                {
                    TrieNode childNode = new TrieNode(child);
                    childNode.addChildList(line.Substring(1));
                    childrenDic.Add(child, childNode);
                }
            }
            childrenList = null;
        }

        public TrieNode getChild(char input)
        {
            if (childrenDic != null)
            {
                if (input == ' ')
                {
                    input = '_';
                }else
                {
                    input = Char.ToLower(input);
                }
                if (childrenDic.ContainsKey(input))
                {
                    return childrenDic[input];
                }
            }
            return null;
            
        }

        public bool isEndOfWord()
        {
            return endOfWord;
        }

        public void setEndOfWord(bool eow)
        {
            endOfWord = eow;
        }
    }
}