using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using HelixToolkit.Wpf;

namespace WpfApp1;
public partial class MainWindowViewModel : ObservableObject
{
    Dictionary<int, (double angleX, double angleY, double angleZ)> FaceRotations = new()
    {
        { 1, (-180, 160, -4) },
        { 2, (-8, 120, 4) },
        { 3, (-170, 90, 40) },
        { 4, (7, 30, 0) },
        { 5, (-180, 30, 8) },
        { 6, (-5, 20, 40) },
        { 7, (-180, -50, -14) },
        { 8, (-15, -100, 10) },
        { 9, (-175, -130, -12) },
        { 10, (-5, -170, 0) },
    };

    Dictionary<int, double> DiceAngleVariability = new()
    {
        { 4, 50 },
        { 6, 40 },
        { 8, 30 },
        { 10, 20 },
        { 12, 15 },
        { 20, 10 },
    };

    [ObservableProperty]
    private double angleX;

    [ObservableProperty]
    private double angleY;

    [ObservableProperty]
    private double angleZ;

    [ObservableProperty]
    private Model3D diceModel;

    [RelayCommand]
    private void RollDice()
    {
        var rand = new Random();
        var xRotationStart = rand.Next(-720, -180);
        var yRotationStart = rand.Next(-720, -180);
        var zRotationStart = rand.Next(-720, -180);

        var rolledNumber = 5;
        var diceFaceOrientation = FaceRotations[rolledNumber];
        var diceOrientationVariability = DiceAngleVariability[10];

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
        var importer = new ModelImporter();
        this.DiceModel = importer.Load(@"Resources\D10.obj");
    }
}
