using System;
using System.Linq;
using System.Text;

namespace IronSharePoint.Util
{
    public interface IBlockingQueue<T>
    {
        int Count { get; }
        bool Enqueue(T item);
        T Dequeue();
        void Stop();
    }
}
