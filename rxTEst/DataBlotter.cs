using System;
using System.Reactive.Subjects;

namespace rxTEst
{
    public class DataBlotter
    {
        BehaviorSubject<object> sink = new BehaviorSubject<object>(null);

        public void Publish<T>(T dto)
        {
            sink.OnNext(dto);
        }

        public IObservable<object> Observe()
        {
            return sink;
        }
    }
}
