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
        { DiceFaces.D6, "Resources\\D6.obj" },
        { DiceFaces.D8, "Resources\\D8.obj" },
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

    private readonly Dictionary<int, (double angleX, double angleY, double angleZ)> d6FaceRotationsDictionary = new()
    {
        { 1, (35, 210, 0) },
        { 2, (-240, 170, -70) },
        { 3, (-50, 140, 50) },
        { 4, (135, -45, 45) },
        { 5, (-105, 140, -180) },
        { 6, (-30, -20, -90) }
    };

    private readonly Dictionary<int, (double angleX, double angleY, double angleZ)> d8FaceRotationsDictionary = new()
    {
        { 1, (20, -15, -15) },
        { 2, (165, 168, -15) },
        { 3, (50, 70, 40) },
        { 4, (175, -115, -18) },
        { 5, (-22, -190, -20) },
        { 6, (190, -20, -20) },
        { 7, (-4, -115, -20) },
        { 8, (200, 75, 0) }
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
        { DiceFaces.D6, 20 },
        { DiceFaces.D8, 15 },
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

    [ObservableProperty]
    private int rollResult;

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
        this.RollResult = Random.Shared.Next(1, diceFacesNumberDictionary[this.SelectedDice]);
        var diceFaceOrientation = diceModelFaceRotationsDictionary[this.SelectedDice][rolledNumber];
            var diceFaceOrientation = diceModelFaceRotationsDictionary[this.SelectedDice][this.RollResult];
        var diceOrientationVariability = diceAngleVariability[this.SelectedDice];

        var diceRollStoryboard = (Storyboard)Application.Current.MainWindow.FindResource("DiceRollStoryboard");
        var rotationX = diceRollStoryboard.Children[0] as DoubleAnimation;
        rotationX.From = Random.Shared.Next(-720, -270);
        rotationX.To = diceFaceOrientation.angleX + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationX.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        var rotationY = diceRollStoryboard.Children[1] as DoubleAnimation;
        rotationY.From = Random.Shared.Next(-720, -270);
        rotationY.To = diceFaceOrientation.angleY + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationY.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        var rotationZ = diceRollStoryboard.Children[2] as DoubleAnimation;
        rotationZ.From = Random.Shared.Next(-720, -270);
        rotationZ.To = diceFaceOrientation.angleZ + (Random.Shared.NextDouble() - 0.5D) * diceOrientationVariability;
        rotationZ.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
        var cameraPoint = diceRollStoryboard.Children[3] as Point3DAnimation;
        var yStartingPosition = Random.Shared.Next(-2, 0) * Random.Shared.NextDouble();
        cameraPoint.From = new Point3D(3, yStartingPosition, 5);

        diceRollStoryboard.Begin();
    }

    public MainWindowViewModel()
    {
        this.modelImporter = new ModelImporter();
        this.diceTypes = [DiceFaces.D4, DiceFaces.D6, DiceFaces.D8, DiceFaces.D10];
        this.SelectedDice = DiceFaces.D4;
        this.diceModelFaceRotationsDictionary = new()
        {
            {DiceFaces.D4, d4FaceRotationsDictionary },
            {DiceFaces.D6, d6FaceRotationsDictionary },
            {DiceFaces.D8, d8FaceRotationsDictionary },
            {DiceFaces.D10, d10FaceRotationsDictionary },
        };
    }
}
