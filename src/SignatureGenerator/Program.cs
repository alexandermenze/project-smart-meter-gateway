using System.Globalization;
using System.Security.Cryptography;
using System.Text;

var requests = new[]
{
    new
    {
        PeriodValue = 1.24, PeriodStart = new DateTime(2024, 10, 16, 19, 15, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 19, 30, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 0.59, PeriodStart = new DateTime(2024, 10, 16, 19, 30, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 19, 45, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 0.32, PeriodStart = new DateTime(2024, 10, 16, 19, 45, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 0, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 1.44, PeriodStart = new DateTime(2024, 10, 16, 20, 0, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 15, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 1.29, PeriodStart = new DateTime(2024, 10, 16, 20, 15, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 30, 0, DateTimeKind.Utc)
    },
    // Second user
    new
    {
        PeriodValue = 1.59, PeriodStart = new DateTime(2024, 10, 16, 19, 30, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 19, 45, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 2.22, PeriodStart = new DateTime(2024, 10, 16, 19, 45, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 0, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 3.83, PeriodStart = new DateTime(2024, 10, 16, 20, 0, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 15, 0, DateTimeKind.Utc)
    },
    new
    {
        PeriodValue = 2.12, PeriodStart = new DateTime(2024, 10, 16, 20, 15, 0, DateTimeKind.Utc),
        PeriodEnd = new DateTime(2024, 10, 16, 20, 30, 0, DateTimeKind.Utc)
    }
};

foreach (var request in requests)
{
    var data = string.Create(CultureInfo.InvariantCulture,
        $"{request.PeriodValue}_{request.PeriodStart:O}_{request.PeriodEnd:O}");

    var dataBytes = Encoding.UTF8.GetBytes(data);

    var privateKey = await File.ReadAllTextAsync("smart-meter-user2.key");

    using var rsaKey = RSA.Create();
    rsaKey.ImportFromPem(privateKey);

    var signature = rsaKey.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    Console.WriteLine(new { Data = data, Signature = Convert.ToBase64String(signature) });
}