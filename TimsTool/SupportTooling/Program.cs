// See https://aka.ms/new-console-template for more information
var treeServices = new TreeServices();
//treeServices.TreeFileTo_DB("D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree.bin");
//treeServices.DBTo_TreeFile("D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree2.bin");
treeServices.FixDupeNRIds("D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree.bin");
treeServices.FixMissingNrs("D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree.bin");
treeServices.Apply1635_1399Changes("D:\\projects\\source\\TimsTool\\TimsTool\\bin\\Debug\\net6.0-windows10.0.17763.0\\ResultsTreeData\\tree.bin");