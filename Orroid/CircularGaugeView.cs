using Android.Content;
using Android.Graphics;
using Android.Views;

namespace Orroid
{
    public class CircularGaugeView : View
    {
        private Paint _backgroundPaint = null!;
        private Paint _progressPaint = null!;
        private RectF _arcRect = null!;
        private float _progress = 0f; // 0.0 ~ 1.0

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Math.Clamp(value, 0f, 1f);
                Invalidate();
            }
        }

        public CircularGaugeView(Context context) : base(context)
        {
            Init();
        }

        public CircularGaugeView(Context context, Android.Util.IAttributeSet? attrs) : base(context, attrs)
        {
            Init();
        }

        public CircularGaugeView(Context context, Android.Util.IAttributeSet? attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        private void Init()
        {
            _backgroundPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Color.DarkGray,
            };
            _backgroundPaint.SetStyle(Paint.Style.Fill);

            _progressPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Color.ParseColor("#4CAF50"),
            };
            _progressPaint.SetStyle(Paint.Style.Fill);

            _arcRect = new RectF();
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            float padding = 10f;
            float size = Math.Min(w, h) - padding * 2;
            float left = (w - size) / 2f;
            float top = (h - size) / 2f;
            _arcRect.Set(left, top, left + size, top + size);
        }

        protected override void OnDraw(Canvas? canvas)
        {
            base.OnDraw(canvas);
            if (canvas is null) return;

            // 背景の円を描画（塗りつぶし）
            canvas.DrawOval(_arcRect, _backgroundPaint);

            // プログレスを描画（-90度から開始 = 12時の位置から時計回りに塗りつぶし）
            float sweepAngle = 360f * _progress;
            canvas.DrawArc(_arcRect, -90, sweepAngle, true, _progressPaint);
        }

        public void SetProgressColor(Color color)
        {
            _progressPaint.Color = color;
            Invalidate();
        }
    }
}
