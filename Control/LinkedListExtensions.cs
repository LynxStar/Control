using System;
using System.Collections.Generic;
using System.Text;

namespace Control
{
    
    public static class LinkedListExtensions
    {

        public static LinkedListNode<T> PreviousBy<T>(this LinkedListNode<T> node, int by)
        {

            var current = node;

            for (int i = 0; i < by; i++)
            {

                if(current.Previous == null)
                {
                    return node;
                }

                current = current.Previous;

            }

            return current;

        }

        public static LinkedListNode<T> NextBy<T>(this LinkedListNode<T> node, int by)
        {

            var current = node;

            for (int i = 0; i < by; i++)
            {

                if (current.Next == null)
                {
                    return node;
                }

                current = current.Next;

            }

            return current;

        }

    }
}
