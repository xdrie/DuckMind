using System;
using XNez.GUtils.Misc;

namespace Ducia.Framework.Utility {
    public abstract class Appraisal<T> {
        protected T context;

        /// <summary>
        /// a utility to attach a "postprocess/transform" function to the score
        /// </summary>
        public Func<float, float>? transform;

        public Appraisal(T context) {
            this.context = context;
        }

        public abstract float score();

        public float transformedScore() {
            var val = score();
            if (transform != null) {
                val = transform(val);
            }

            return val;
        }

        public Appraisal<T> negate() {
            transform += v => -v;
            return this;
        }

        public Appraisal<T> inverse() {
            transform += v => (1f - v);
            return this;
        }

        public Appraisal<T> clamp(float limit) {
            transform += v => GMathf.clamp(v, 0, limit);
            return this;
        }
    }
}