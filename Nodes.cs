using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        internal List<int> Keys { get; set; }//llaves de los nodos
        internal List<T> Datos { get; set; }//datos del nodo

        //devuelve el numero de datos que contiene el Arbol;
        internal int NumeroDatos
        {
            get
            {
                int i = 0;
                while (i < Keys.Count && Keys[i] != Utilities.ApuntadorVacio)
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
                    if (Children[i] != Utilities.ApuntadorVacio)
                    {
                        Leaf = false;
                        break;
                    }
                }
                return Leaf;
            }
        }

        // obtiene el tamaño del nodo en texto
        internal int SizeInText
        {
            get
            {
                int tamañoEnTexto = 0;

                tamañoEnTexto += Utilities.TextoEnteroTamaño + 1;
                tamañoEnTexto += Utilities.TextoEnteroTamaño + 1;
                tamañoEnTexto += 2;
                tamañoEnTexto += (Utilities.TextoEnteroTamaño + 1) * Orden;
                tamañoEnTexto += 2;
                tamañoEnTexto += (Utilities.TextoEnteroTamaño + 1) * (Orden - 1);
                tamañoEnTexto += 2;
                tamañoEnTexto += (Datos[0].TamañoEnTexto + 1) * (Orden - 1);
                tamañoEnTexto += Utilidades.TextoNuevaLineaTamaño;
                return tamañoEnTexto;
            }
        }

        //devuelve el tamaño de la memoria utilizada en bytes
        internal int MemoryInBytes
        {
            get
            {
                return SizeInText * Utilities.BinarioCaracterTamaño;
            }
        }
        //calcula la posicion en la que esta el disco
        private int CalcularPosicionEnDisco(int tamañoEncabezado)
        {
            return tamañoEncabezado + (Position * MemoryInBytes);
        }

        //convertir a tamano fijo
        private string ConvertirATextoTamañoFijo()
        {
            StringBuilder datosCadena = new StringBuilder();

            datosCadena.Append(Utilities.FormatearEntero(Position));
            datosCadena.Append(Utilities.TextoSeparador);
            datosCadena.Append(Utilities.FormatearEntero(Father));
            datosCadena.Append(Utilities.TextoSeparador);
            datosCadena.Append(Utilities.TextoSeparador);
            datosCadena.Append(Utilities.TextoSeparador);

            for (int i = 0; i < Children.Count; i++)
            {
                datosCadena.Append(Utilities.FormatearEntero(Children[i]));
                datosCadena.Append(Utilities.TextoSeparador);
            }
            datosCadena.Append(Utilities.TextoSeparador);
            datosCadena.Append(Utilities.TextoSeparador);

            for (int i = 0; i < Keys.Count; i++)
            {
                datosCadena.Append(Utilities.FormatearEntero(Keys[i]));
                datosCadena.Append(Utilities.TextoSeparador);
            }

            datosCadena.Append(Utilities.TextoSeparador);
            datosCadena.Append(Utilities.TextoSeparador);

            for (int i = 0; i < Datos.Count; i++)
            {
                datosCadena.Append(Datos[i].ConvertirATextoTamañoFijo().Replace(Utilities.TextoSeparador, Utilities.TextoSustitutoSeparador));
                datosCadena.Append(Utilidades.TextoSeparador);
            }

            datosCadena.Append(Utilities.TextoNuevaLinea);

            return datosCadena.ToString();
        }

        //obtiene los bytes en el texto de tamaño fijo
        private byte[] ObtenerBytes()
        {
            byte[] datosBinarios = null;
            datosBinarios = Utilities.ConvertirBinarioYTexto(ConvertirATextoTamañoFijo());
            return datosBinarios;
        }

        private void LimpiarNodo(IFabricaTextoTamañoFijo<T> fabrica)
        {
            Children = new List<int>();
            for (int i = 0; i < Orden; i++)
            {
                Children.Add(Utilities.ApuntadorVacio);
            }

            Keys = new List<int>();
            for (int i = 0; i < Orden - 1; i++)
            {
                Keys.Add(Utilities.ApuntadorVacio);
            }

            Datos = new List<T>();
            for (int i = 0; i < Orden - 1; i++)
            {
                Datos.Add(fabrica.FabricarNulo());
            }
        }
        //Constructor
        internal Nodes(int orden, int posicion, int padre, IFabricaTextoTamañoFijo<T> fabrica)
        {
            if ((orden < MinOrder) || (orden > MaxOrder))
            {
                throw new ArgumentOutOfRangeException("orden");
            }

            if (posicion < 0)
            {
                throw new ArgumentOutOfRangeException("posicion");
            }

            Orden = orden; Position = posicion;
            Father = padre;

            LimpiarNodo(fabrica);
        }

        //lee el nodo ya existente en el dico
        internal static Nodes<T, Tkey> ReadNodesInDisk(FileStream archivo, int tamañoEncabezado, int orden, int posicion, IFabricaTextoTamañoFijo<T> fabrica)
        {
            if (archivo == null)
            {
                throw new ArgumentNullException("archivo");
            }

            if (tamañoEncabezado < 0)
            {
                throw new ArgumentOutOfRangeException("tamañoEncabezado");
            }

            if ((orden < MinOrder) || (orden > MaxOrder))
            {
                throw new ArgumentOutOfRangeException("orden");
            }

            if (posicion < 0)
            {
                throw new ArgumentOutOfRangeException("posicion");
            }
            if (fabrica == null)
            {
                throw new ArgumentNullException("fabrica");
            }

            Nodes<T, Tkey> newNode = new Nodes<T, Tkey>(orden, posicion, 0, fabrica);
            byte[] datosBinario = new byte[newNode.MemoryInBytes];

            string datosCadena = "";
            string[] datosSeparados = null;
            int PosicionEnDatosCadena = 1;
            archivo.Seek(newNode.CalcularPosicionEnDisco(tamañoEncabezado), SeekOrigin.Begin);
            archivo.Read(datosBinario, 0, newNode.MemoryInBytes);
            datosCadena = Utilities.ConvertirBinarioYTexto(datosBinario);

            datosCadena = datosCadena.Replace(Utilities.TextoNuevaLinea, "");
            datosCadena = datosCadena.Replace("".PadRight(3, Utilities.TextoSeparador), Utilities.TextoSeparador.ToString());
            datosSeparados = datosCadena.Split(Utilities.TextoSeparador);

            newNode.Father = Convert.ToInt32(datosSeparados[PosicionEnDatosCadena]);
            PosicionEnDatosCadena++;

            for (int i = 0; i < newNode.Keys.Count; i++)
            {
                newNode.Keys[i] = Convert.ToInt32(datosSeparados[PosicionEnDatosCadena]);
                PosicionEnDatosCadena++;
            }

            for (int i = 0; i < newNode.Keys.Count; i++)
            {
                newNode.Keys[i] = Convert.ToInt32(datosSeparados[PosicionEnDatosCadena]);
                PosicionEnDatosCadena++;
            }

            for (int i = 0; i < newNode.Datos.Count; i++)
            {
                datosSeparados[PosicionEnDatosCadena] = datosSeparados[PosicionEnDatosCadena].Replace(Utilidades.TextoSustitutoSeparador, Utilidades.TextoSeparador);
                newNode.Datos[i] = fabrica.Fabricar(datosSeparados[PosicionEnDatosCadena]); PosicionEnDatosCadena++;
            }
            return newNode;
        }

        internal void GuardarNodoEnDisco(FileStream archivo, int tamañoEncabezado)
        {             // Se ubica la posición donde se debe escribir             
            archivo.Seek(CalcularPosicionEnDisco(tamañoEncabezado), SeekOrigin.Begin);

            // Se escribe al archivo y se fuerza a vaciar el buffer             
            archivo.Write(ObtenerBytes(), 0, MemoryInBytes);
            archivo.Flush();
        }

        internal void LimpiarNodoEnDisco(FileStream archivo, int tamañoEncabezado, IFabricaTextoTamañoFijo<T> fabrica)
        {
            LimpiarNodo(fabrica);


            GuardarNodoEnDisco(archivo, tamañoEncabezado);
        }


        internal int PosicionAproximadaEnNodo(int llave)
        {
            int posicion = Keys.Count;

            for (int i = 0; i < Keys.Count; i++)
            {
                if ((Keys[i] > llave) || (Keys[i] == Utilities.ApuntadorVacio))
                {
                    posicion = i; break;
                }
            }
            return posicion;
        }
        internal int PosicionExactaEnNodo(int llave)
        {
            int posicion = -1;
            for (int i = 0; i < Keys.Count; i++) { if (llave == Keys[i]) { posicion = i; break; } }

            return posicion;
        }

        internal void AgregarDato(int llave, T dato, int hijoDerecho)
        {
            AgregarDato(llave, dato, hijoDerecho, true);
        }


        internal void AgregarDato(int llave, T dato, int hijoDerecho, bool ValidarLleno)
        {
            if (Overflow && ValidarLleno)
            {
                throw new IndexOutOfRangeException("El nodo está lleno, ya no puede insertar más datos");
            }

            if (llave == Utilities.ApuntadorVacio)
            {
                throw new ArgumentOutOfRangeException("llave");
            }

            int posicionParaInsertar = 0;
            posicionParaInsertar = PosicionAproximadaEnNodo(llave);

            for (int i = Children.Count - 1; i > posicionParaInsertar + 1; i--)
            {
                Children[i] = Children[i - 1];
            }
            Children[posicionParaInsertar + 1] = hijoDerecho;

            for (int i = Keys.Count - 1; i > posicionParaInsertar; i--)
            {
                Keys[i] = Keys[i - 1];
                Datos[i] = Datos[i - 1];
            }
            Keys[posicionParaInsertar] = llave; Datos[posicionParaInsertar] = dato;
        }

        internal void AgregarDato(int llave, T dato)
        {
            AgregarDato(llave, dato, Utilities.ApuntadorVacio);
        }

        internal void EliminarDato(int llave)
        {
            if (!Leaf)
            {
                throw new Exception("Solo pueden eliminarse llaves en nodos hoja");
            }

            int posicionParaEliminar = -1;
            posicionParaEliminar = PosicionExactaEnNodo(llave);

            if (posicionParaEliminar == -1)
            {
                throw new ArgumentException("No puede eliminarse ya que no existe la llave en el nodo");
            }

            for (int i = Keys.Count - 1; i > posicionParaEliminar; i--)
            {
                Keys[i - 1] = Keys[i];
                Datos[i - 1] = Datos[i];
            }
            Keys[Keys.Count - 1] = Utilities.ApuntadorVacio;
        }
        internal void SepararNodo(int llave, T dato, int hijoDerecho, Nodes<T, Tkey> nuevoNodo, ref int llavePorSubir, T datoPorSubir)
        {
            if (!Overflow)
            {
                throw new Exception("Uno nodo solo puede separarse si está lleno");

            }
            Keys.Add(Utilities.ApuntadorVacio);
            Datos.Add(dato);
            Children.Add(Utilities.ApuntadorVacio);
            AgregarDato(llave, dato, hijoDerecho, false);
            int mitad = (Orden / 2); llavePorSubir = Keys[mitad];
            datoPorSubir = Datos[mitad];
            Keys[mitad] = Utilities.ApuntadorVacio;

            int j = 0; for (int i = mitad + 1; i < Keys.Count; i++)
            {
                nuevoNodo.Keys[j] = Keys[i];
                nuevoNodo.Datos[j] = Datos[i];

                Keys[i] = Utilities.ApuntadorVacio;

                j++;
            }
            j = 0;
            for (int i = mitad + 1; i < Children.Count; i++)
            {
                nuevoNodo.Children[j] = Keys[i];

                Children[i] = Utilities.ApuntadorVacio;

                j++;
            }

            Keys.RemoveAt(Keys.Count - 1);
            Datos.RemoveAt(Datos.Count - 1);
            Children.RemoveAt(Children.Count - 1);
        }
    }
}


