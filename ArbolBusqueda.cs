using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Laboratorio3_ED
{
    public abstract class ArbolBusqueda<TLlave, T> where TLlave : IComparable
    {

        public int Tamaño { get; protected set; }

        public abstract void Agregar(TLlave llave, T dato);

        public abstract void Eliminar(TLlave llave);

        public abstract string RecorrerPreOrden();

    }
}
