   M S B A M L   `   `   `  ����G  CTpmMigration, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Mclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration?Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration  5      ]    WMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.SchemaMigrationPageView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ftpmMigrationMclr-namespace:Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration     5        � 	Resources  5	      m  gMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.MigrationStatusEnumToImagePathConverter +  'migrationStatusEnumToImagePathConvertera  [Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.BoolToOppositeBoolConverter  boolToOppositeBoolConverter_  YMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.NullToVisibilityConverter  nullToVisibilityConverter��%W   &	        &	      &	 &       5
   
   6?     5   
   63     5   
   61   5      .��5      � -   5        $��Layout��5      $
��Left=�6   $	��Top=�64    � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   All Schemas can be found �� 5   &   -   5         ��Name$ FileLink��5   0    �� Command 6B   �� *OpenSchemaDirCommand+.�6k   here6q   5      &   System.Windows.Controls.DataGrid  5   
   -   5        $��SchemaSelectionDataGrid��5      $	��700��62     StandardDataGrid#��[ 5      $��1��5      ��5      �� *MigrationItems+ �� Mode$ OneWay=�   Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   CanUserReorder .    CanUserResize .    CanUserSort . 	   
IsReadOnly	 . 
   Width5  /System.Windows.Controls.DataGridLengthConverter$	
 90* 5!   5   X   RMicrosoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.DataGridExtensions   
HeaderName 5   %   �� *SchemaNameColumnHeader+  	Resources �� Source# [ 1	  +System.Windows.Controls.DataGridBoundColumn 	  Binding 5   5   �� * MigrationEntity.schemaFullName+5!   5     5#       .  .  . 	 . $	
 60* 5&   5    5#   %   �� *AssemblynameColumnHeader+# [  5$   5   �� *,*MigrationEntity.assemblyFullyQualifiedName+5&   5     5(       .  .  . 	 . $	
 40* 5+   5    5(   %   �� *VersionColumnHeader+# [  5)   5   �� *MigrationEntity.version+5+   5   4
  .System.Windows.Controls.DataGridTemplateColumn
  5-      	 . $
 70 6;    6F   �� *MigrationStatusColumnHeader+# [  
  CellTemplate 5.      �� 5/      .R�50      �� $��15��6�   ��6$   �� *ImportStatus+ �� 	Converter# [   � ToolTip 51   "   _� 52   &   $	��700��6�   y�6.   �� *ImportStatusText+# [ .��53   *   �� $
��Wrap=�6V   ��64   �� *ImportStatusText+6V   54   '   56      57      59      5;      5<      5=      