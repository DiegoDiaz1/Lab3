using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;


namespace Laboratorio3_ED
{
    public class ArbolB<T> : ArbolBusqueda<int, T> where T : ITextoTamañoFijo
    {
        private const int Encabezado = 5 * Utilities.EnteroYEnterBinarioTamaño;


        private int raiz;
        private int UltimaPosLibre;


        private FileStream archivo = null;
        private string NombreArchivo = "";
        private IFabricaTextoTamañoFijo<T> Creador = null;


        public int Orden { get; set; }
        public int Altura { get; set; }


        private void GuardarEncabezado()
        {
            Utilities.EscribirEntero(archivo, 0, raiz);
            Utilities.EscribirEntero(archivo, 1, UltimaPosLibre);
            Utilities.EscribirEntero(archivo, 2, Tamaño);
            Utilities.EscribirEntero(archivo, 3, Orden);
            Utilities.EscribirEntero(archivo, 4, Altura);
            archivo.Flush();
        }

        private void Agregar(int posNodoActual, int llave, T dato)
        {
            Nodes<T, llave> nodoActual = Nodes<T, llave>.ReadNodesInDisk(archivo, Encabezado, Orden, posNodoActual, Creador);
            if (nodoActual.PosicionExactaEnNodo(llave) != -1)
            {
                throw new InvalidOperationException("La llave indicada ya está contenida en el árbol.");
            }

            if (nodoActual.Leaf)
            {
                Subir(nodoActual, llave, dato, Utilities.ApuntadorVacio);
                GuardarEncabezado();
            }
            else
            {
                Agregar(nodoActual.Children[nodoActual.PosicionAproximadaEnNodo(llave)], llave, dato);
            }
        }

        private void Subir(Nodes<T, Tkey> nodoActual, int llave, T dato, int hijoDerecho)
        {
            if (!nodoActual.Overflow)
            {
                nodoActual.AgregarDato(llave, dato, hijoDerecho); nodoActual.GuardarNodoEnDisco(archivo, Encabezado);
                return;
            }

            Nodes<T, Tkey> Hermano = new Nodes<T, int>(Orden, UltimaPosLibre, nodoActual.Father, Creador);
            UltimaPosLibre++;

            int llavePorsubir = Utilities.ApuntadorVacio;
            T datoPorSubir = Creador.CrearNulo();
            nodoActual.SepararNodo(llave, dato, hijoDerecho, Hermano, ref llavePorsubir, datoPorSubir);

            Nodes<T, int> Hijo = null;
            for (int i = 0; i < Hermano.Children.Count; i++)
            {
                if (Hermano.Children[i] != Utilities.ApuntadorVacio)
                {
                    Hijo = Nodes<T, int>.ReadNodesInDisk(archivo, Encabezado, Orden, Hermano.Children[i], Creador);
                    Hijo.Father = Hermano.Position;
                    Hijo.GuardarNodoEnDisco(archivo, Encabezado);
                }
                else
                {
                    break;
                }
            }
            if (nodoActual.Father == Utilities.ApuntadorVacio)
            {
                Nodes<T, int> nuevaRaiz = new Nodes<T, int>(Orden, UltimaPosLibre, Utilities.ApuntadorVacio, Creador);
                UltimaPosLibre++;
                Altura++;

                nuevaRaiz.Children[0] = nodoActual.Position;
                nuevaRaiz.AgregarDato(llavePorsubir, datoPorSubir, Hermano.Position);


                nodoActual.Father = nuevaRaiz.Position;
                Hermano.Father = nuevaRaiz.Position;
                raiz = nuevaRaiz.Position;


                nuevaRaiz.GuardarNodoEnDisco(archivo, Encabezado);
                Hermano.GuardarNodoEnDisco(archivo, Encabezado);
                Hermano.GuardarNodoEnDisco(archivo, Encabezado);

            }
            else
            {
                nodoActual.GuardarNodoEnDisco(archivo, Encabezado);
                Hermano.GuardarNodoEnDisco(archivo, Encabezado);


                Nodes<T, int> nodoPadre = Nodes<T, int>.ReadNodesInDisk(archivo, Encabezado, Orden, nodoActual.Father, Creador);
                Subir(nodoPadre, llavePorsubir, datoPorSubir, Hermano.Position);
            }
        }


        public ArbolB(int orden, string nombreArchivo, IFabricaTextoTamañoFijo<T> _Creador)
        {
            NombreArchivo = nombreArchivo;
            Creador = _Creador;

            archivo = new FileStream(nombreArchivo, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            raiz = Utilities.LeerEntero(archivo, 0);
            UltimaPosLibre = Utilities.LeerEntero(archivo, 1);
            Tamaño = Utilities.LeerEntero(archivo, 3);
            orden = Utilities.LeerEntero(archivo, 4);

            if (UltimaPosLibre ==Utilities.ApuntadorVacio)
            {
                UltimaPosLibre = 0;
            }
            if (Tamaño == Utilities.ApuntadorVacio)
            {
                Tamaño = 0;
            }
            if (Orden == Utilities.ApuntadorVacio)
            {
                Orden = orden;
            }

            if (Altura == Utilities.ApuntadorVacio)
            {
                Altura = 1;
            }

            if (raiz == Utilities.ApuntadorVacio)
            {
                Nodes<T, int> nodoCabeza = new Nodes<T, int>(Orden, UltimaPosLibre, Utilities.ApuntadorVacio, Creador);
                UltimaPosLibre++;
                raiz = nodoCabeza.Position;
                nodoCabeza.GuardarNodoEnDisco(archivo, Encabezado);

            }

            GuardarEncabezado();
        }


        public override void Agregar(int llave, TLlave dato)
        {
            if (llave == Utilities.ApuntadorVacio)
            {
                throw new ArgumentOutOfRangeException("Llave");
            }
            Agregar(raiz, llave, dato);
            Tamaño++;
        }


    }
}
