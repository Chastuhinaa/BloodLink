using BloodLink.Domain;

namespace BloodLink.Api.Data;

// Демо-дані: назви й координати центрів умовні.
public static class SeedData
{
    public static readonly BloodCenter[] Centers =
    {
        new() { Id = 1, Name = "Центр крові «Поділ» (демо)", Address = "вул. Сагайдачного, 10", City = "Київ", Latitude = 50.4627, Longitude = 30.5196, Phone = "+380440000001", WorkingHours = "Пн–Пт 08:00–13:00" },
        new() { Id = 2, Name = "Центр крові «Лівобережний» (демо)", Address = "просп. Броварський, 20", City = "Київ", Latitude = 50.4521, Longitude = 30.5979, Phone = "+380440000002", WorkingHours = "Пн–Сб 08:00–14:00" },
        new() { Id = 3, Name = "Центр крові «Голосіїв» (демо)", Address = "просп. Голосіївський, 30", City = "Київ", Latitude = 50.3960, Longitude = 30.5101, Phone = "+380440000003", WorkingHours = "Пн–Пт 08:00–12:00" },
        new() { Id = 4, Name = "Центр крові «Шулявка» (демо)", Address = "просп. Берестейський, 40", City = "Київ", Latitude = 50.4555, Longitude = 30.4460, Phone = "+380440000004", WorkingHours = "Вт–Сб 08:30–13:30" },
        new() { Id = 5, Name = "Центр крові «Оболонь» (демо)", Address = "просп. Оболонський, 50", City = "Київ", Latitude = 50.5132, Longitude = 30.4983, Phone = "+380440000005", WorkingHours = "Пн–Пт 08:00–13:00" },
    };
}
