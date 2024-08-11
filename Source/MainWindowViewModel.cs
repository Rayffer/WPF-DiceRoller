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
        { DiceFaces.D100, "Resources\\D100.obj" },
    };

    private readonly Dictionary<DiceFaces, int> diceFacesNumberDictionary = new()
    {
        { DiceFaces.D4, 5 },
        { DiceFaces.D6, 7 },
        { DiceFaces.D8, 9 },
        { DiceFaces.D10, 11 },
        { DiceFaces.D12, 13 },
        { DiceFaces.D20, 21 },
        { DiceFaces.D100, 101 },
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
        { 0, (0, -175, 0) },
        { 1, (180, 165, 0) },
        { 2, (0, 110, 0) },
        { 3, (180, 95, 0) },
        { 4, (0, 40, 0) },
        { 5, (-180, 20, 0) },
        { 6, (0, -33, 0) },
        { 7, (-180, -50, 0) },
        { 8, (0, -102, 0) },
        { 9, (-180, -125, 0) },
        { 10, (0, -175, 0) },
    };

    private readonly Dictionary<int, (double angleX, double angleY, double angleZ)> d100FaceRotationsDictionary = new()
    {
        { 0, (180, 110, 0) },
        { 1, (0, -50, 0) },
        { 2, (180, 40, 0) },
        { 3, (-5, -123, -5) },
        { 4, (180, -33, 0) },
        { 5, (0, 163, 0) },
        { 6, (180, -104, 0) },
        { 7, (0, 93, 0) },
        { 8, (180, 180, 0) },
        { 9, (0, 23, 0) },
    };

    private readonly Dictionary<DiceFaces, double> diceAngleVariability = new()
    {
        { DiceFaces.D4, 10 },
        { DiceFaces.D6, 20 },
        { DiceFaces.D8, 10 },
        { DiceFaces.D10, 10 },
        { DiceFaces.D12, 10 },
        { DiceFaces.D20, 10 },
        { DiceFaces.D100, 10 },
    };

    [ObservableProperty]
    private double angleX;

    [ObservableProperty]
    private double angleY;

    [ObservableProperty]
    private double angleZ;

    [ObservableProperty]
    private double d100AngleX;

    [ObservableProperty]
    private double d100AngleY;

    [ObservableProperty]
    private double d100AngleZ;

    [ObservableProperty]
    private int rollResult;

    public Model3D DiceModel
    {
        get
        {
            if (this.SelectedDice == DiceFaces.D100)
            {
                return this.modelImporter.Load(this.diceModelDictionary[DiceFaces.D10]);

            }
            return this.modelImporter.Load(this.diceModelDictionary[this.SelectedDice]);
        }
    }

    public double OffsetX => this.SelectedDice is DiceFaces.D100 ? 1 : 0;
    public double OffsetZ => this.SelectedDice is DiceFaces.D100 ? -0.5 : 0;

    public double D100OffsetX => this.SelectedDice is DiceFaces.D100 ? 0.5 : 0;
    public double D100OffsetZ => this.SelectedDice is DiceFaces.D100 ? -0.5 : 0;

    public Model3D DiceModelD100
    {
        get
        {
            if (this.SelectedDice != DiceFaces.D100)
            {
                return null;
            }
            return this.modelImporter.Load(this.diceModelDictionary[this.SelectedDice]);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor("DiceModel")]
    [NotifyPropertyChangedFor("DiceModelD100")]
    [NotifyPropertyChangedFor("OffsetX")]
    [NotifyPropertyChangedFor("OffsetZ")]
    [NotifyPropertyChangedFor("D100OffsetX")]
    [NotifyPropertyChangedFor("D100OffsetZ")]
    private DiceFaces selectedDice;

    [ObservableProperty]
    private ObservableCollection<DiceFaces> diceTypes;

    [RelayCommand]
    private void RollDice()
    {
        if (this.SelectedDice is DiceFaces.D100)
        {
            this.RollResult = Random.Shared.Next(0, 100);

            var unitsRolledNumber = this.RollResult % 10;
            var d10FaceOrientation = diceModelFaceRotationsDictionary[DiceFaces.D10][unitsRolledNumber];
            var d10OrientationVariability = diceAngleVariability[DiceFaces.D10];

            var tensRolledNumber = this.RollResult / 10;
            var d100FaceOrientation = diceModelFaceRotationsDictionary[DiceFaces.D100][tensRolledNumber];
            var d100OrientationVariability = diceAngleVariability[DiceFaces.D100];

            var diceRollStoryboard = (Storyboard)Application.Current.MainWindow.FindResource("DiceRollStoryboard");
            var rotationX = diceRollStoryboard.Children[0] as DoubleAnimation;
            rotationX.From = Random.Shared.Next(-720, -270);
            rotationX.To = d10FaceOrientation.angleX + (Random.Shared.NextDouble() - 0.5D) * d10OrientationVariability;
            rotationX.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var rotationY = diceRollStoryboard.Children[1] as DoubleAnimation;
            rotationY.From = Random.Shared.Next(-720, -270);
            rotationY.To = d10FaceOrientation.angleY + (Random.Shared.NextDouble() - 0.5D) * d10OrientationVariability;
            rotationY.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var rotationZ = diceRollStoryboard.Children[2] as DoubleAnimation;
            rotationZ.From = Random.Shared.Next(-720, -270);
            rotationZ.To = d10FaceOrientation.angleZ + (Random.Shared.NextDouble() - 0.5D) * d10OrientationVariability;
            rotationZ.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var translate = diceRollStoryboard.Children[3] as DoubleAnimation;
            translate.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            diceRollStoryboard.Begin();

            var d100DiceRollStoryboard = (Storyboard)Application.Current.MainWindow.FindResource("D100DiceRollStoryboard");
            var d100RotationX = d100DiceRollStoryboard.Children[0] as DoubleAnimation;
            d100RotationX.From = Random.Shared.Next(-720, -270);
            d100RotationX.To = d100FaceOrientation.angleX + (Random.Shared.NextDouble() - 0.5D) * d100OrientationVariability;
            d100RotationX.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var d100RotationY = d100DiceRollStoryboard.Children[1] as DoubleAnimation;
            d100RotationY.From = Random.Shared.Next(-720, -270);
            d100RotationY.To = d100FaceOrientation.angleY + (Random.Shared.NextDouble() - 0.5D) * d100OrientationVariability;
            d100RotationY.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var d100RotationZ = d100DiceRollStoryboard.Children[2] as DoubleAnimation;
            d100RotationZ.From = Random.Shared.Next(-720, -270);
            d100RotationZ.To = d100FaceOrientation.angleZ + (Random.Shared.NextDouble() - 0.5D) * d100OrientationVariability;
            d100RotationZ.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            var translateD100 = d100DiceRollStoryboard.Children[3] as DoubleAnimation;
            translateD100.Duration = new Duration(TimeSpan.FromSeconds(1).Add(TimeSpan.FromSeconds(Random.Shared.NextDouble() * 0.75)));
            d100DiceRollStoryboard.Begin();
        }
        else
        {
            this.RollResult = Random.Shared.Next(1, diceFacesNumberDictionary[this.SelectedDice]);

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
            diceRollStoryboard.Begin();
        }
    }

    public MainWindowViewModel()
    {
        this.modelImporter = new ModelImporter();
        this.diceTypes = [DiceFaces.D4, DiceFaces.D6, DiceFaces.D8, DiceFaces.D10, DiceFaces.D100];
        this.SelectedDice = DiceFaces.D4;
        this.diceModelFaceRotationsDictionary = new()
        {
            {DiceFaces.D4, d4FaceRotationsDictionary },
            {DiceFaces.D6, d6FaceRotationsDictionary },
            {DiceFaces.D8, d8FaceRotationsDictionary },
            {DiceFaces.D10, d10FaceRotationsDictionary },
            {DiceFaces.D100, d100FaceRotationsDictionary },
        };
    }
}
