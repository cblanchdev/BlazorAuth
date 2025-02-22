# BlazorAuth
BlazorAuth is an authentication library that works on pages using InteractiveSerer rendering.
BlazorAuth uses the SignInManager and UserManager from IdentityFramework for authentication as well as ProtectedSessionStorage and ProtectedLocalStorage to maintain session state.

## Installation
Register BlazorAuth in the Program.cs file. You can specify Session or Local storage. Session storage is the default value.
```csharp
builder.Services.AddBlazorAuth(options => options.StorageType = BlazorAuthOptions.eStorageType.Session);
```

Add BlazorAuth to _Imports.razor
```csharp
@using BlazorAuth
```

## Usage
Inject AuthenticationStateProvider into your component.
```csharp
[Inject] private AuthenticationStateProvider AuthState { get; set; } = default!;
```

Cast the AuthenticationStateProvider to the BlazorAuth object for the SignIn and SignOut methods
```csharp
SignInResult signInResult = await ((BlazorAuth)AuthState).SignIn(Input.Email, Input.Password);
await ((BlazorAuth)AuthState).SignOut();
```
