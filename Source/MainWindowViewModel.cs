using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HelixToolkit.Wpf;

namespace Rayfer.DiceRoller.WPF;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly ModelImporter modelImporter;
    private readonly Dictionary<DiceFaces, Dictionary<int, (double angleX, double angleY, double angleZ)>> diceModelFaceRotationsDictionary;

    private readonly Dictionary<DiceFaces, string> diceModelDictionary = new()
    {
        { DiceFaces.D4, "Resources\\D4.obj" },
        { DiceFaces.D10, "Resources\\D10.obj" },
    };

    private readonly Dictionary<DiceFaces, int> diceFacesNumberDictionary = new()
    {
        { DiceFaces.D4, 5 },
        { DiceFaces.D6, 7 },
        { DiceFaces.D8, 9 },
        { DiceFaces.D10, 11 },
        { DiceFaces.D12, 13 },
        { DiceFaces.D20, 21 },
    };


    private readonly Dictionary<int, (double angleX, double angleY, double angleZ)> d4FaceRotationsDictionary = new()
    {
        { 1, (-60, 95, 15) },
        { 2, (-5, -20, 60) },
        { 3, (180, 40, 15) },
        { 4, (60, 25, -10) }
    };

    private readonly Dictionary<int, (double angleX, double angleY, double angleZ)> d10FaceRotationsDictionary = new()
    {
        { 1, (-180, 160, -4) },
        { 2, (-8, 120, 4) },
        { 3, (-170, 90, 20) },
        { 4, (7, 30, 0) },
        { 5, (-180, 30, 8) },
        { 6, (-5, 20, 40) },
        { 7, (-180, -50, -14) },
        { 8, (-15, -100, 10) },
        { 9, (-175, -130, -12) },
        { 10, (-5, -170, 0) },
    };

    private readonly Dictionary<DiceFaces, double> diceAngleVariability = new()
    {
        { DiceFaces.D4, 15 },
        { DiceFaces.D6, 40 },
        { DiceFaces.D8, 30 },
        { DiceFaces.D10, 20 },
        { DiceFaces.D12, 15 },
        { DiceFaces.D20, 10 },
    };

    private Model3D diceModel;

    [ObservableProperty]
    private double angleX;

    [ObservableProperty]
    private double angleY;

    [ObservableProperty]
    private double angleZ;

    public Model3D DiceModel
    {
        get
        {
            return this.diceModel = this.modelImporter.Load(this.diceModelDictionary[this.SelectedDice]);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor("DiceModel")]
    private DiceFaces selectedDice;

    [ObservableProperty]
    private ObservableCollection<DiceFaces> diceTypes;

    [RelayCommand]
    private void RollDice()
    {
        var rand = new Random();
        var xRotationStart = rand.Next(-720, -270);
        var yRotationStart = rand.Next(-720, -270);
        var zRotationStart = rand.Next(-720, -270);

        var rolledNumber = Random.Shared.Next(1, diceFacesNumberDictionary[this.SelectedDice]);
        var diceFaceOrientation = diceModelFaceRotationsDictionary[this.SelectedDice][rolledNumber];
        var diceOrientationVariability = diceAngleVariability[this.SelectedDice];

        var diceRollStoryboard = (Storyboard)Application.Current.MainWindow.FindResource("DiceRollStoryboard");
        var rotationX = diceRollStoryboard.Children[0] as DoubleAnimation;
        rotationX.From = xRotationStart;
        rotationX.To = diceFaceOrientation.angleX + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationX.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        var rotationY = diceRollStoryboard.Children[1] as DoubleAnimation;
        rotationY.From = yRotationStart;
        rotationY.To = diceFaceOrientation.angleY + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationY.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        var rotationZ = diceRollStoryboard.Children[2] as DoubleAnimation;
        rotationZ.From = zRotationStart;
        rotationZ.To = diceFaceOrientation.angleZ + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationZ.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        diceRollStoryboard.Begin();
    }

    public MainWindowViewModel()
    {
        this.modelImporter = new ModelImporter();
        this.diceTypes = [DiceFaces.D4, DiceFaces.D10];
        this.SelectedDice = DiceFaces.D4;
        this.diceModelFaceRotationsDictionary = new()
        {
            {DiceFaces.D4, d4FaceRotationsDictionary },
            {DiceFaces.D10, d10FaceRotationsDictionary },
        };
    }
}
