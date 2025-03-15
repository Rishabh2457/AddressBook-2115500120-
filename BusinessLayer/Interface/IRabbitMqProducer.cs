using System;
using ModelLayer.DTO;
namespace BusinessLayer.Interface
{
    public interface IRabbitMqProducer
    {
        public void PublishMessage(UserEventDTO userEvent);
    }
}
