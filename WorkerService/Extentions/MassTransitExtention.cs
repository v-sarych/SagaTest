using Domain.Model.Requestes;
using MassTransit;

namespace WorkerService.Extentions
{
    public static class MassTransitExtention
    {
        public static IServiceCollection AddMassTransit(this IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq((bus, rabbit) =>
                {
                    rabbit.Host("192.168.0.14", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    rabbit.Send<CreateOrderSagaRequest>();
                    rabbit.ConfigureEndpoints(bus);
                });
            });

            return services;
        }
    }
}
