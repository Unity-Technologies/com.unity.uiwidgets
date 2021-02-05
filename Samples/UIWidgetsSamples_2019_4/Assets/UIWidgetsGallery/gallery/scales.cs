using System;
using System.Collections.Generic;
using Unity.UIWidgets.material;

namespace UIWidgetsGallery.gallery
{
    public class GalleryTextScaleValue : IEquatable<GalleryTextScaleValue>
    {
        public GalleryTextScaleValue(float? scale, string label)
        {
            this.scale = scale;
            this.label = label;
        }
        
        public readonly float? scale;
        public readonly string label;


        public bool Equals(GalleryTextScaleValue other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return this.scale.Equals(other.scale) && this.label == other.label;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            
            if (obj.GetType() != GetType())
            {
                return false;
            } 
            
            return Equals((GalleryTextScaleValue)obj);
        }
        
        public static bool operator==(GalleryTextScaleValue left, GalleryTextScaleValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryTextScaleValue left, GalleryTextScaleValue right)
        {
            return !Equals(left, right);
        }
        
        public override int GetHashCode() {
            unchecked {
                return ((label?.GetHashCode() ?? 0) * 397) ^ (scale?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return $"{GetType()}{label}";
        }
        
        public static readonly List<GalleryTextScaleValue> kAllGalleryTextScaleValues = new List<GalleryTextScaleValue>{
            new GalleryTextScaleValue(null, "System Default"),
            new GalleryTextScaleValue(0.8f, "Small"),
            new GalleryTextScaleValue(1.0f, "Normal"),
            new GalleryTextScaleValue(1.3f, "Large"),
            new GalleryTextScaleValue(2.0f, "Huge"),
        };
    }

    public class GalleryVisualDensityValue : IEquatable<GalleryVisualDensityValue>
    {
        public GalleryVisualDensityValue(VisualDensity visualDensity, string label)
        {
            this.visualDensity = visualDensity;
            this.label = label;
        }
        
        public readonly VisualDensity visualDensity;
        public readonly string label;
        
        public bool Equals(GalleryVisualDensityValue other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return this.visualDensity.Equals(other.visualDensity) && this.label == other.label;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            
            if (obj.GetType() != GetType())
            {
                return false;
            } 
            
            return Equals((GalleryVisualDensityValue)obj);
        }
        
        public static bool operator==(GalleryVisualDensityValue left, GalleryVisualDensityValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GalleryVisualDensityValue left, GalleryVisualDensityValue right)
        {
            return !Equals(left, right);
        }
        
        public override int GetHashCode() {
            unchecked {
                return ((label?.GetHashCode() ?? 0) * 397) ^ (visualDensity?.GetHashCode() ?? 0);
            }
        }

        public override string ToString()
        {
            return $"{GetType()}{label}";
        }
        
        public static readonly List<GalleryVisualDensityValue> kAllGalleryVisualDensityValues = new List<GalleryVisualDensityValue>{
            new GalleryVisualDensityValue(new VisualDensity(), "System Default"),
            new GalleryVisualDensityValue(VisualDensity.comfortable, "Comfortable"),
            new GalleryVisualDensityValue(VisualDensity.compact, "Compact"),
            new GalleryVisualDensityValue(new VisualDensity(horizontal: -3, vertical: -3), "Very Compact")
        };
    }
}