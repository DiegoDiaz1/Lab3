using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab._3
{
    class Nodes<T, Tkey> where Tkey : IComparable<T>
    {
        public const int MinOrder = 3;//Orden minimo del arbol
        public const int MaxOrder = 99;//Orden Maximo del arbol
        internal int Orden { get; private set; }//Orden del arbol
        internal int Position { get; private set; }//posicion en el arbol
        internal int Father { get; set; }// padre del nodo
        internal List<int> Children { get; set; }//hijos del nodo
        internal List<Tkey> Keys { get; set; }//llaves de los nodos
        internal List<T> Datos { get; set; }//datos del nodo

        //devuelve el numero de datos que contiene el Arbol;
        internal int NumeroDatos
        {
            get
            {
                int i = 0;
                while (i < Keys.Count && Keys[i] != Utilidades.ApuntadorVacio)
                {
                    i++;
                }
                return i;
            }
        }
        //devuelve un valor de verdadero o falso si el nodo contiene 0 datos;
        internal bool Underflow
        {
            get
            {
                return (NumeroDatos < ((Orden / 2) - 1));
            }
        }

        // devuelve un valor de verdadero o falso si el nodo contiene mas datos de los que el orden del arbol permite;
        internal bool Overflow
        {
            get
            {
                return (NumeroDatos >= Orden - 1);
            }
        }

        //devuelve un valor de verdadero o falso si el nodo es una hoja o no;
        internal bool Leaf
        {
            get
            {
                bool Leaf = true;

                for (int i = 0; i < Children.Count; i++)
                {
                    if (Children[i] != Utilidades.ApuntadorVacio)
                    {
                        Leaf = false;
                        break;
                    }
                }
                return Leaf;
            }
        }
    }
}