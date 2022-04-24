
namespace GLFrameworkEngine
{
    /// <summary>
    /// Drag drop events for when something is drag/dropped onto a rendered 3D object.
    /// The object also must use an IPickable interface.
    /// </summary>
    public interface IDragDropPicking
    {
        //When something outside has dropped onto the picking object (ie a material onto a mesh)
        void DragDroppedOnLeave();
        void DragDroppedOnEnter();
        void DragDropped(object droppedItem);
    }
}
