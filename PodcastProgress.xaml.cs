namespace Ruquier;

public partial class PodcastProgress : ContentView
{
  public PodcastProgress()
  {
    InitializeComponent();
    UpdateBarWidth();
  }

  public static readonly BindableProperty ProgressProperty =
      BindableProperty.Create(nameof(Progress), typeof(double), typeof(PodcastProgress), 0.0, propertyChanged: OnProgressChanged);

  public static readonly BindableProperty MaxWidthProperty =
      BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(PodcastProgress), 150.0, propertyChanged: OnProgressChanged);

  public static readonly BindableProperty BarHeightProperty =
      BindableProperty.Create(nameof(BarHeight), typeof(double), typeof(PodcastProgress), 12.0);

  public double Progress
  {
    get => (double)GetValue(ProgressProperty);
    set => SetValue(ProgressProperty, value);
  }

  public double MaxWidth
  {
    get => (double)GetValue(MaxWidthProperty);
    set => SetValue(MaxWidthProperty, value);
  }

  public double BarHeight
  {
    get => (double)GetValue(BarHeightProperty);
    set => SetValue(BarHeightProperty, value);
  }

  public double BarWidth => MaxWidth * Progress;

  private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
  {
    if (bindable is PodcastProgress bar)
      bar.UpdateBarWidth();
  }

  private void UpdateBarWidth()
  {
    OnPropertyChanged(nameof(BarWidth));
  }
}