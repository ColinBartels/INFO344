using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;

namespace Query_Suggestion
{
    public class Trie
    {
        private TrieNode root;

        public Trie()
        {
            root = new TrieNode(' ');
        }

        public void addLine(string line)
        {
             createTrieRecursion(root, line);
        }

        private void createTrieRecursion(TrieNode node, string line)
        {
            if (node.isTrie())
            {
                if (line.Length == 0)
                {
                    node.setEndOfWord(true);
                    return;
                }

                char current = line[0];
                if (node.getChild(current) == null)
                {
                    node.addChildDic(current);
                }
                createTrieRecursion(node.getChild(current), line.Substring(1));
            }
            else
            {
                
                if (line.Length == 0)
                {
                    node.setEndOfWord(true);
                }
                else
                {
                    node.addChildList(line);
                }
            }

        }

        private TrieNode matchPrefix(TrieNode node, string prefix)
        {
            if (prefix.Length == 0)
            {
                return node;
            }
            char current = prefix[0];
            if (node.isTrie())
            {
                TrieNode child = node.getChild(current);
                if (child == null)
                {
                    return null;
                }
                else
                {
                    return matchPrefix(child, prefix.Substring(1));
                }
            }
            else
            {
                List<string> children = node.getChildrenList();
                List<string> matches = new List<string>();
                foreach (string child in children)
                {
                    if (child.Length >= prefix.Length)
                    {
                        bool match = true;
                        for (int i = 0; i < prefix.Length; i++)
                        {
                            if (!prefix[i].Equals(child[i]))
                            {
                                match = false;
                                break;
                            }
                        }
                        if (match)
                        {
                            matches.Add(child.Substring(prefix.Length));
                        }
                    }
                }
                if (matches.Count == 0)
                {
                    return null;
                }
                else
                {
                    TrieNode result = new TrieNode(prefix[prefix.Length - 1]);
                    foreach (string match in matches)
                    {
                        result.addChildList(match);
                    }
                    return result;
                }
            }
        }

        public List<String> findCompletions(string prefix)
        {
            TrieNode match = matchPrefix(root, prefix);
            List<string> completions = new List<string>();
            if (match != null)
            {
                findCompletionsRecursive(match, prefix, completions);
            }
            return completions;
        }

        private void findCompletionsRecursive(TrieNode node, string prefix, List<string> completions)
        {
            if (node == null || completions.Count >= 10)
            {
                return;
            }
            if (node.isEndOfWord())
            {
                completions.Add(prefix);
            }
            if (node.isTrie())
            {
                List<TrieNode> children = node.getChildrenDic();
                if (children != null)
                {
                    foreach (TrieNode child in children)
                    {
                        char childChar = child.getChar();
                        findCompletionsRecursive(child, prefix + childChar, completions);
                    }
                }
            }
            else
            {
                List<string> matches = node.getChildrenList();
                List<string> editedMatches = new List<string>();
                foreach (string match in matches)
                {
                    if(completions.Count + editedMatches.Count < 10)
                    {
                        editedMatches.Add(prefix + match);
                    }
                }
                completions.AddRange(editedMatches);
            }
            return;
        }

    }
}