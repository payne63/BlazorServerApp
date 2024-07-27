using Microsoft.AspNetCore.Components;
using Radzen;

namespace BlazorServerApp.Pages;
    
public partial class TestComponent: RadzenComponentWithChildren{
    private string someText;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        var faker = new Bogus.Faker(locale:"fr");
        someText = faker.Person.Email ;

    }

    [Parameter] public int val { get; set; } = 4;
}
