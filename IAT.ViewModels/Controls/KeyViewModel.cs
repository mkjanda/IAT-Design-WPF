using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IAT.ViewModels.Controls;

public partial class KeyViewModel : ObservableObject
{
    private readonly IKeyedServiceProvider _keyService;
}
